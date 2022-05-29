using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalonAPI.Models;
using SalonAPI.Models.DTOs;
using System.Data;
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


        [HttpGet("BookingsByUser")]
        public async Task<ActionResult<List<BookingDTO>>> GetBookingsByUser(int userId)
        {
            //you can only get your own Bookings unless you are an admin
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentUserId = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString();
            var currentUserRole = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value.ToString();

            if(!currentUserRole.Equals(Roles.Admin.GetString()))
            {
                if (!currentUserId.Equals(userId.ToString())) return Unauthorized("Not allowed to get this user's bookings");
            }

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

            var employee = await context.Employees.Where(x => x.Id == bookingDTO.EmployeeId).FirstOrDefaultAsync();

            //MAking sure that the formatting of the DTO is correct
            bookingDTO.StartTime = new DateTime(bookingDTO.StartTime.Year, bookingDTO.StartTime.Month, bookingDTO.StartTime.Day,
                bookingDTO.StartTime.Hour, bookingDTO.StartTime.Minute, 0);

            bookingDTO.EndTime = bookingDTO.StartTime.AddMinutes(service.DurationInMinutes);


            var isValid = true;
            var error = "";
            IsValid(bookingDTO, service, employee, out isValid, out error);

            if (!isValid) return BadRequest(error);

            var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var dbBookings = await context.Bookings
                .Include(x => x.Employee)
                .Include(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.EmployeeId == bookingDTO.EmployeeId)
                .Where(x => x.StartTime.Day == bookingDTO.StartTime.Day)
                .ToListAsync();

                var isOverlapping = false;
                
                if (service.PauseStartInMinutes != null && service.PauseEndInMinutes != null)
                {
                    //We need to treat this booking as two separate bookings with each their own time ranges.
                    //This way it is possible to create another booking in the pause time of the service
                    //and we can use the same logic to detect overlapp with other services.

                    var bookingDTOBeforePause = new BookingDTO()
                    {
                        BookedById = bookingDTO.Id,
                        EmployeeId = bookingDTO.EmployeeId,
                        ServiceId = bookingDTO.ServiceId,
                        StartTime = bookingDTO.StartTime,
                        EndTime = bookingDTO.StartTime.AddMinutes(Convert.ToDouble(service.PauseStartInMinutes))
                    };

                    var bookingDTOAfterPause = new BookingDTO()
                    {
                        BookedById = bookingDTO.Id,
                        EmployeeId = bookingDTO.EmployeeId,
                        ServiceId = bookingDTO.ServiceId,   
                        StartTime = bookingDTO.StartTime.AddMinutes(Convert.ToDouble(service.PauseEndInMinutes)),
                        EndTime = bookingDTO.StartTime.AddMinutes(Convert.ToDouble(service.DurationInMinutes))
                    };

                    IsOverlapping(bookingDTOBeforePause.StartTime, bookingDTOBeforePause.EndTime, dbBookings, out isOverlapping);
                    IsOverlapping(bookingDTOAfterPause.StartTime, bookingDTOAfterPause.EndTime, dbBookings, out isOverlapping);
                    if (isOverlapping) return BadRequest("Booking is overlapping with another booking already in the database");

                    var bookingBeforePause = new Booking()
                    {
                        BookedById = currentUserId,
                        User = user,
                        EmployeeId = bookingDTO.EmployeeId,
                        Employee = employee,
                        ServiceId = bookingDTO.ServiceId,
                        Service = service,
                        StartTime = bookingDTOBeforePause.StartTime,
                        EndTime = bookingDTOBeforePause.EndTime
                    };

                    var bookingAfterPause = new Booking()
                    {
                        BookedById = currentUserId,
                        User = user,
                        EmployeeId = bookingDTO.EmployeeId,
                        Employee = employee,
                        ServiceId = bookingDTO.ServiceId,
                        Service = service,
                        StartTime = bookingDTOAfterPause.StartTime,
                        EndTime = bookingDTOAfterPause.EndTime
                    };


                    context.Bookings.Add(bookingBeforePause);
                    context.Bookings.Add(bookingAfterPause);
                }
                else
                {
                    IsOverlapping(bookingDTO.StartTime, bookingDTO.EndTime, dbBookings, out isOverlapping);
                    if (isOverlapping) return BadRequest("Booking is overlapping with another booking already in the database");

                    var booking = new Booking()
                    {
                        BookedById = currentUserId,
                        User = user,
                        EmployeeId = bookingDTO.EmployeeId,
                        Employee = employee,
                        ServiceId = bookingDTO.ServiceId,
                        Service = service,
                        StartTime = bookingDTO.StartTime,
                        EndTime = bookingDTO.EndTime
                    };

                    context.Bookings.Add(booking);
                }
                
                await context.SaveChangesAsync();

                //Returner kun dine bookings
                var bookingsDTO = await context.Bookings.Where(x => x.BookedById == user.Id)
                    .Select(x => Mapper.MapToDTO(x)).ToListAsync();

                transaction.Commit();

                return Ok(bookingsDTO);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
        }

        private void IsValid(BookingDTO booking, Service service, Employee employee, out bool isValid, out string error)
        {
            isValid = true;
            error = "";

            //checking that the employee exists
            if (employee == null) { isValid = false; error = "Employee could not be found"; return; }


            //Checking that the service exists and that the employee is actually doing this service
            if (service == null) { isValid = false; error = "Service could not be found"; return; }

            var serviceHasEmployee = false;
            foreach (var serviceEmployee in service.Employees)
            {
                if (serviceEmployee.Id == booking.EmployeeId) serviceHasEmployee = true; break;
            }

            if (!serviceHasEmployee) { isValid = false; error = "Service does not have employee"; return; }


            //Checking if date is not in the past
            if (DateTime.Compare(booking.StartTime, DateTime.Now) < 0) { isValid = false; error = "Datetime cannot be in the past"; return; }



            //Checking if the datetime is free for the current employee
            //Basically we create a range from the booking time to bookingtime+service duration
            //Then we check if any other booking.datetime for this employee falls within this range. 
            //How do we handle the endpoints for this range?
            //The start is inclusive, the endpoint is not
            //So this means that the endpoint is starttime + (servicetime - 1 min)
            //Remember to implement transaction isolation!
            //We need to check when other bookings end.
            //Basically we need to make sure that this range does not overlap with anothr booking's range.

            //Getting all bookings for this employee today


            //TODO: remember to use transaction isolation here



            //not same bookedbyid SOLVED
            //Employee and Service can potentially not be matching. SOLVED
            //ids can be pointing to nonexistent entities. SOLVED

            //When creating, make sure date is not in the past SOLVED
            //The date booked must not overlap with other bookibgs
            //The booking must be within opening hours for salon
            //The booking can't be between two days
            //Check that the date is valid (like 35th of june should not be valid.)

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
