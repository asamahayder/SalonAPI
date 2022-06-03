using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalonAPI.Models;
using SalonAPI.Models.DTOs;
using System.Data;
using System.Globalization;
using System.Security.Claims;

namespace SalonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {

        private readonly DataContext context;

        public BookingController(DataContext context)
        {
            this.context = context;

        }

        [HttpGet("BookingsByEmployee")]
        public async Task<ActionResult<List<BookingDTO>>> GetBookingsByEmployee(int employeeId)
        {
            var bookings = await context.Bookings
                .Include(x => x.Employee)
                .Include(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.EmployeeId == employeeId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();
            return Ok(bookings);
        }

        [HttpGet("GetMyBookings")]
        public async Task<ActionResult<List<BookingDTO>>> GetMyBookings()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentUserId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);

            var bookings = await context.Bookings
                .Include(x => x.Employee)
                .Include(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.BookedById == currentUserId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(bookings);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("BookingsByUser")]
        public async Task<ActionResult<List<BookingDTO>>> GetBookingsByUser(int userId)
        {
            var bookings = await context.Bookings
                .Include(x => x.Employee)
                .Include(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.BookedById == userId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(bookings);
        }


        [HttpGet("id")]
        [Authorize(Roles="Admin")]
        public async Task<ActionResult<BookingDTO>> Get(int Id)
        {
            var booking = await context.Bookings
                .Include(x => x.Employee)
                .Include(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.BookedById == Id)
                .Select(x => Mapper.MapToDTO(x)).FirstOrDefaultAsync();

            if (booking == null)
            {
                return BadRequest("Booking not found");
            }

            return Ok(booking);
        }


        [HttpPost("CreateBooking")]
        public async Task<ActionResult<List<BookingDTO>>> CreateBooking(BookingDTO bookingDTO)
        {
            //Getting user identity
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var userEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var currentUserIdString = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString();
            var currentUserId = Int32.Parse(currentUserIdString);
            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == userEmail);

            
            var service = await context.Services.Include(x => x.Employees)
                .Where(x => x.Id == bookingDTO.ServiceId).FirstOrDefaultAsync();

            if (service == null) return NotFound("Service could not be found");

            var employee = await context.Employees.Where(x => x.Id == bookingDTO.EmployeeId).FirstOrDefaultAsync();

            if (employee == null) return NotFound("Employee could not be found");


            //Removing seconds and milliseconds from starttime and creating a new endtime in case the user has specified 
            //an invalid end time.
            bookingDTO.StartTime = new DateTime(bookingDTO.StartTime.Year, bookingDTO.StartTime.Month, bookingDTO.StartTime.Day,
                bookingDTO.StartTime.Hour, bookingDTO.StartTime.Minute, 0);

            bookingDTO.EndTime = bookingDTO.StartTime.AddMinutes(service.DurationInMinutes);



            //Checking if the booking is within the employees opening hours for this day.
            //First checking if there are special opening days for this week.
            DateTime? startTimeForDay = null;
            DateTime? endTimeForDay = null;
            bool isOpenForDay = false;

            var weekday = bookingDTO.StartTime.DayOfWeek;

            var specialOpeningHours = await context.SpecialOpeningHours
                .FirstOrDefaultAsync(x => x.EmployeeId == employee.Id 
                && x.Week.Year == bookingDTO.StartTime.Year 
                && ISOWeek.GetWeekOfYear(x.Week) == ISOWeek.GetWeekOfYear(bookingDTO.StartTime));

            if (specialOpeningHours == null)
            {
                var openingHours = await context.OpeningHours.FirstOrDefaultAsync(x => x.EmployeeId == employee.Id);
                openingHours.GetDay(weekday, out var startTime, out var endTime, out var isOpen);
                startTimeForDay = startTime;
                endTimeForDay = endTime;
                isOpenForDay = isOpen;

            }
            else
            {
                specialOpeningHours.GetDay(weekday, out var startTime, out var endTime, out var isOpen);
                startTimeForDay = startTime;
                endTimeForDay = endTime;
                isOpenForDay = isOpen;
            }

            if (startTimeForDay == null || endTimeForDay == null) return BadRequest("Something went wrong with retrieving opening hours");

            if (!isOpenForDay) return BadRequest("Employee not working this day");

            if (bookingDTO.StartTime.CompareTo(startTimeForDay) < 0 || bookingDTO.EndTime.CompareTo(startTimeForDay) <= 0) 
               return BadRequest("Booking start/end time cant be outside the start of the day");

            if (bookingDTO.StartTime.CompareTo(endTimeForDay) >= 0 || bookingDTO.EndTime.CompareTo(endTimeForDay) > 0)
                return BadRequest("Booking start/end time cant be outside the end of the day");

            IsValid(bookingDTO.StartTime, employee.Id, service, out bool isValid, out string? error);

            if (!isValid) return BadRequest(error);

            var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            SaveBooking(user, employee, bookingDTO, service, out var success, out var errorMessage);

            if (success)
            {
                var bookingsDTO = await context.Bookings.Where(x => x.BookedById == user.Id)
                    .Select(x => Mapper.MapToDTO(x)).ToListAsync();

                transaction.Commit();

                return Ok(bookingsDTO);
            }
            else
            {
                transaction.Rollback();
                return BadRequest(errorMessage);
            }
            
        }


        [HttpPut("UpdateBooking")]
        public async Task<ActionResult<List<BookingDTO>>> UpdateBooking(BookingDTO bookingDTO)
        {
            //Getting user identity
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var userEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var currentUserIdString = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString();
            var currentUserId = Int32.Parse(currentUserIdString);
            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == userEmail);

            var dbBooking = await context.Bookings
                .Include(x => x.Employee)
                .Include(x => x.Service)
                .Include(x => x.User)
                .Where(x => x.Id == bookingDTO.Id).FirstOrDefaultAsync();

            if (dbBooking == null) return NotFound("Could not find booking");

            var service = await context.Services.Include(x => x.Employees)
                .Where(x => x.Id == bookingDTO.ServiceId).FirstOrDefaultAsync();

            if (service == null) return NotFound("Service could not be found");

            var employee = await context.Employees.Where(x => x.Id == bookingDTO.EmployeeId).FirstOrDefaultAsync();

            if (employee == null) return NotFound("Employee could not be found");


            //Removing seconds and milliseconds from starttime and creating a new endtime in case the user has specified 
            //an invalid end time.
            bookingDTO.StartTime = new DateTime(bookingDTO.StartTime.Year, bookingDTO.StartTime.Month, bookingDTO.StartTime.Day,
                bookingDTO.StartTime.Hour, bookingDTO.StartTime.Minute, 0);

            bookingDTO.EndTime = bookingDTO.StartTime.AddMinutes(service.DurationInMinutes);

            //Checking if the booking is within the employees opening hours for this day.
            //First checking if there are special opening days for this week.
            DateTime? startTimeForDay = null;
            DateTime? endTimeForDay = null;
            bool isOpenForDay = false;

            var weekday = bookingDTO.StartTime.DayOfWeek;
            
            //We have to check both week and year, as bookings from different years can have the same week number.
            var specialOpeningHours = await context.SpecialOpeningHours
                .FirstOrDefaultAsync(x => x.EmployeeId == employee.Id 
                && x.Week.Year == bookingDTO.StartTime.Year 
                && ISOWeek.GetWeekOfYear(x.Week) == ISOWeek.GetWeekOfYear(bookingDTO.StartTime));

            if (specialOpeningHours == null)
            {
                var openingHours = await context.OpeningHours.FirstOrDefaultAsync(x => x.EmployeeId == employee.Id);
                openingHours.GetDay(weekday, out var startTime, out var endTime, out var isOpen);
                startTimeForDay = startTime;
                endTimeForDay = endTime;
                isOpenForDay = isOpen;
            }
            else
            {
                specialOpeningHours.GetDay(weekday, out var startTime, out var endTime, out var isOpen);
                startTimeForDay = startTime;
                endTimeForDay = endTime;
                isOpenForDay = isOpen;
            }

            if (startTimeForDay == null || endTimeForDay == null) return BadRequest("Something went wrong with retrieving opening hours");

            if (!isOpenForDay) return BadRequest("Employee not working this day");

            if (bookingDTO.StartTime.CompareTo(startTimeForDay) < 0 || bookingDTO.EndTime.CompareTo(startTimeForDay) <= 0)
                return BadRequest("Booking start/end time cant be outside the start of the day");

            if (bookingDTO.StartTime.CompareTo(endTimeForDay) >= 0 || bookingDTO.EndTime.CompareTo(endTimeForDay) > 0)
                return BadRequest("Booking start/end time cant be outside the end of the day");

            IsValid(bookingDTO.StartTime, employee.Id, service, out bool isValid, out string? error);

            if (!isValid) return BadRequest(error);

            if (!CheckPermission(user, dbBooking)) return Unauthorized("No permission to edit this booking");

            var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            context.Bookings.Remove(dbBooking);

            context.SaveChanges();

            SaveBooking(user, employee, bookingDTO, service, out var success, out var errorMessage);

            if (success)
            {
                var bookingsDTO = await context.Bookings.Where(x => x.BookedById == user.Id)
                    .Select(x => Mapper.MapToDTO(x)).ToListAsync();

                transaction.Commit();

                return Ok(bookingsDTO);
            }
            else
            {
                transaction.Rollback();
                return BadRequest(errorMessage);
            }

        }


        [HttpDelete("DeleteBooking")]
        public async Task<ActionResult<List<BookingDTO>>> DeleteBooking(int id)
        {
            //Admin can delete any booking
            //Owner can only delete bookings from his salons
            //Employee can only delete his bookings
            //Users can only delete their boookings

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentUserId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var currentUser = await context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);

            var dbBooking = await context.Bookings
                .Include(x => x.Employee)
                .Include(x => x.Service)
                .Include(x => x.User)
                .Where(x => x.Id == id).FirstOrDefaultAsync();

            if (dbBooking == null) return NotFound("Could not find booking");


            if (!CheckPermission(currentUser, dbBooking)) return Unauthorized("No permission to edit this booking");


            context.Bookings.Remove(dbBooking);
            await context.SaveChangesAsync();

            var bookingsDTO = await context.Bookings.Where(x => x.BookedById == currentUserId)
                    .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(bookingsDTO);
        }


        private void SaveBooking(User user, Employee employee, BookingDTO bookingDTO, Service service, out bool success, out string? errorMessage)
        {
            success = false;
            errorMessage = null;

            
            try
            {
                var dbBookings = context.Bookings
                .Include(x => x.Employee)
                .Include(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.EmployeeId == bookingDTO.EmployeeId)
                .Where(x => x.StartTime.Day == bookingDTO.StartTime.Day)
                .ToList();

                var isOverlapping = false;

                if (service.PauseStartInMinutes != null && service.PauseEndInMinutes != null)
                {
                    //We need to treat this booking as two separate bookings with each their own time ranges.
                    //This way it is possible to create another booking in the pause time of the service
                    //and we can use the same logic to detect overlapp with other services.

                    var bookingDTOBeforePause = new BookingDTO()
                    {
                        BookedById = user.Id,
                        EmployeeId = bookingDTO.EmployeeId,
                        ServiceId = bookingDTO.ServiceId,
                        StartTime = bookingDTO.StartTime,
                        EndTime = bookingDTO.StartTime.AddMinutes(Convert.ToDouble(service.PauseStartInMinutes)),
                        Note = bookingDTO.Note
                    };

                    var bookingDTOAfterPause = new BookingDTO()
                    {
                        BookedById = user.Id,
                        EmployeeId = bookingDTO.EmployeeId,
                        ServiceId = bookingDTO.ServiceId,
                        StartTime = bookingDTO.StartTime.AddMinutes(Convert.ToDouble(service.PauseEndInMinutes)),
                        EndTime = bookingDTO.StartTime.AddMinutes(Convert.ToDouble(service.DurationInMinutes)),
                        Note = bookingDTO.Note
                    };

                    IsOverlapping(bookingDTOBeforePause.StartTime, bookingDTOBeforePause.EndTime, dbBookings, out isOverlapping);
                    IsOverlapping(bookingDTOAfterPause.StartTime, bookingDTOAfterPause.EndTime, dbBookings, out isOverlapping);
                    if (isOverlapping) { throw new BookingOverlapException("Booking is overlapping with an already existing booking"); }

                    var bookingBeforePause = new Booking()
                    {
                        BookedById = user.Id,
                        User = user,
                        EmployeeId = bookingDTO.EmployeeId,
                        Employee = employee,
                        ServiceId = bookingDTO.ServiceId,
                        Service = service,
                        StartTime = bookingDTOBeforePause.StartTime,
                        EndTime = bookingDTOBeforePause.EndTime,
                        Note = bookingDTOBeforePause.Note
                    };

                    var bookingAfterPause = new Booking()
                    {
                        BookedById = user.Id,
                        User = user,
                        EmployeeId = bookingDTO.EmployeeId,
                        Employee = employee,
                        ServiceId = bookingDTO.ServiceId,
                        Service = service,
                        StartTime = bookingDTOAfterPause.StartTime,
                        EndTime = bookingDTOAfterPause.EndTime,
                        Note = bookingDTOAfterPause.Note
                    };


                    context.Bookings.Add(bookingBeforePause);
                    context.Bookings.Add(bookingAfterPause);
                }
                else
                {
                    IsOverlapping(bookingDTO.StartTime, bookingDTO.EndTime, dbBookings, out isOverlapping);
                    if (isOverlapping) throw new BookingOverlapException("Booking is overlapping with an already existing booking"); 

                    var booking = new Booking()
                    {
                        BookedById = user.Id,
                        User = user,
                        EmployeeId = bookingDTO.EmployeeId,
                        Employee = employee,
                        ServiceId = bookingDTO.ServiceId,
                        Service = service,
                        StartTime = bookingDTO.StartTime,
                        EndTime = bookingDTO.EndTime,
                        Note = bookingDTO.Note
                    };

                    context.Bookings.Add(booking);
                }

                context.SaveChanges();

                success = true;
            }
            catch (Exception e)
            {
                success = false;
                errorMessage = e.Message;
            }

            
        }

        /// <summary>
        /// Checks if user has permission to edit booking.
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="dbBooking"></param>
        /// <returns></returns>
        private bool CheckPermission(User currentUser, Booking dbBooking)
        {
            var hasPermission = false;

            if (currentUser.Role == Roles.Admin) hasPermission = true;

            if (currentUser.Role == Roles.Owner)
            {
                var dbEmployee =  context.Employees.Include(x => x.Salon).FirstOrDefault(x => x.Id == dbBooking.EmployeeId);
                if (dbEmployee.Salon.OwnerId == currentUser.Id) hasPermission = true;
            }

            if (currentUser.Role == Roles.Employee && dbBooking.EmployeeId == currentUser.Id) hasPermission = true;


            if (dbBooking.BookedById == currentUser.Id) hasPermission = true;

            return hasPermission;
        }

        private void IsValid(DateTime startTime, int employeeId, Service service, out bool isValid, out string error)
        {
            isValid = true;
            error = "";


            //Checking that the employee is actually doing this service
            var serviceHasEmployee = false;
            foreach (var serviceEmployee in service.Employees)
            {
                if (serviceEmployee.Id == employeeId) serviceHasEmployee = true; break;
            }

            if (!serviceHasEmployee) { isValid = false; error = "Service does not have employee"; return; }



            //Checking if date is not in the past
            if (DateTime.Compare(startTime, DateTime.Now) < 0) { isValid = false; error = "Datetime cannot be in the past"; return; }
           
        }

        /// <summary>
        /// Checks if a given time range is overlapping with any existing booking's time range.
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="dbBookings"></param>
        /// <param name="isOverlapping"></param>
        private void IsOverlapping(DateTime startTime, DateTime endTime, List<Booking> dbBookings, out bool isOverlapping)
        {
            isOverlapping = false;

            foreach (var dbBooking in dbBookings)
            {

                if (DateTime.Compare(dbBooking.StartTime, endTime) < 0 && DateTime.Compare(startTime, dbBooking.EndTime) < 0)
                {
                    isOverlapping = true;
                    break;
                }

            }

        }

    }
}
