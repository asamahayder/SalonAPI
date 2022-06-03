using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalonAPI.Models;
using SalonAPI.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;

namespace SalonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpeningHoursController : ControllerBase
    {

        private readonly DataContext context;

        public OpeningHoursController(DataContext context)
        {
            this.context = context;
        }

        [HttpGet("GetOpeningHoursForEmployeeByWeek")]
        public async Task<ActionResult<OpeningHoursDTO>> GetOpeningHours(int employeeID, DateTime week)
        {
            var employee = await context.Employees.FirstOrDefaultAsync(x => x.Id == employeeID);
            if (employee == null) return NotFound("Could not find employee");

            var specialOpeningHours = await context.SpecialOpeningHours
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeID && x.Week.Year == week.Year && ISOWeek.GetWeekOfYear(x.Week) == ISOWeek.GetWeekOfYear(week));

            
            if (specialOpeningHours == null)
            {
                var openingHours = await context.OpeningHours.FirstOrDefaultAsync(x => x.EmployeeId == employeeID);
                return Mapper.MapToDTO(openingHours);
            }
            else
            {
                return Mapper.MapToOpeningHoursDTO(specialOpeningHours);
            }
        }

        [HttpGet("GetNormalHours")]
        public async Task<ActionResult<OpeningHoursDTO>> GetNormalHours(int employeeID)
        {
            var employee = await context.Employees.FirstOrDefaultAsync(x => x.Id == employeeID);
            if (employee == null) return NotFound("Could not find employee");

            var openingHours = await context.OpeningHours.FirstOrDefaultAsync(x => x.EmployeeId == employeeID);
                return Mapper.MapToDTO(openingHours);
        }

        [HttpGet("GetSpecialHoursByWeek")]
        public async Task<ActionResult<SpecialOpeningHoursDTO>> GetSpecialHoursByWeek(int employeeID, DateTime week)
        {
            var employee = await context.Employees.FirstOrDefaultAsync(x => x.Id == employeeID);
            if (employee == null) return NotFound("Could not find employee");

            var specialOpeningHours = await context.SpecialOpeningHours.
                FirstOrDefaultAsync(x => x.EmployeeId == employeeID && x.Week.Year == week.Year && ISOWeek.GetWeekOfYear(x.Week) == ISOWeek.GetWeekOfYear(week));

            if (specialOpeningHours == null) return NotFound("No special hours for this week");

            return Mapper.MapToDTO(specialOpeningHours);
        }

        

        [HttpPost("CreateSpecialOpeningHours"), Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<SpecialOpeningHoursDTO>>> CreateSpecialOpeningHours(SpecialOpeningHoursDTO specialOpeningHoursDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentEmployeeId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var currentEmployee = await context.Employees.FirstOrDefaultAsync(x => x.Id == currentEmployeeId);

            if (specialOpeningHoursDTO.EmployeeId != currentEmployeeId) return Unauthorized("Can't edit another employee's opening hours.");


            //check if already existing opening hours for given week

            var existingSpecialOpeningHours = await context.SpecialOpeningHours
                .FirstOrDefaultAsync(x => x.EmployeeId == currentEmployeeId 
                && x.Week.Year == specialOpeningHoursDTO.Week.Year 
                && ISOWeek.GetWeekOfYear(x.Week) == ISOWeek.GetWeekOfYear(specialOpeningHoursDTO.Week));

            if (existingSpecialOpeningHours != null) BadRequest("There already exists a specialOpeningHours for this week.");

            var newSpecialOpeningHours = new SpecialOpeningHours()
            {
                Employee = currentEmployee,
                EmployeeId = currentEmployeeId,
                Week = specialOpeningHoursDTO.Week,
                MondayOpen =  specialOpeningHoursDTO.MondayOpen,
                MondayStart =  CreateDateTime(specialOpeningHoursDTO.MondayStart),
                MondayEnd = CreateDateTime(specialOpeningHoursDTO.MondayEnd),
                TuesdayOpen = specialOpeningHoursDTO.TuesdayOpen,
                TuesdayStart = CreateDateTime(specialOpeningHoursDTO.TuesdayStart),
                TuesdayEnd = CreateDateTime(specialOpeningHoursDTO.TuesdayEnd),
                WednessdayOpen = specialOpeningHoursDTO.WednessdayOpen,
                WednessdayStart = CreateDateTime(specialOpeningHoursDTO.WednessdayStart),
                WednessdayEnd = CreateDateTime(specialOpeningHoursDTO.WednessdayEnd),
                ThursdayOpen = specialOpeningHoursDTO.ThursdayOpen,
                ThursdayStart = CreateDateTime(specialOpeningHoursDTO.ThursdayStart),
                ThursdayEnd = CreateDateTime(specialOpeningHoursDTO.ThursdayEnd),
                FridayOpen = specialOpeningHoursDTO.FridayOpen,
                FridayStart = CreateDateTime(specialOpeningHoursDTO.FridayStart),
                FridayEnd = CreateDateTime(specialOpeningHoursDTO.FridayEnd),
                SaturdayOpen = specialOpeningHoursDTO.SaturdayOpen,
                SaturdayStart = CreateDateTime(specialOpeningHoursDTO.SaturdayStart),
                SaturdayEnd = CreateDateTime(specialOpeningHoursDTO.SaturdayEnd),
                SundayOpen = specialOpeningHoursDTO.SundayOpen,
                SundayStart = CreateDateTime(specialOpeningHoursDTO.SundayStart),
                SundayEnd = CreateDateTime(specialOpeningHoursDTO.SundayEnd),
            };

            context.SpecialOpeningHours.Add(newSpecialOpeningHours);

            await context.SaveChangesAsync();

            var dtos = await context.SpecialOpeningHours.Where(x => x.EmployeeId == currentEmployeeId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return dtos;

        }


        [HttpPost("UpdateSpecialOpeningHours"), Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<SpecialOpeningHoursDTO>>> UpdateSpecialOpeningHours(SpecialOpeningHoursDTO specialOpeningHoursDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentEmployeeId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var currentEmployee = await context.Employees.FirstOrDefaultAsync(x => x.Id == currentEmployeeId);

            if (specialOpeningHoursDTO.EmployeeId != currentEmployeeId) return Unauthorized("Can't edit another employee's opening hours.");

            var dbSpecialOpeningHours = await context.SpecialOpeningHours
                .FirstOrDefaultAsync(x => x.EmployeeId == specialOpeningHoursDTO.EmployeeId 
                && x.Week.Year == specialOpeningHoursDTO.Week.Year 
                && ISOWeek.GetWeekOfYear(x.Week) == ISOWeek.GetWeekOfYear(specialOpeningHoursDTO.Week));

            if (dbSpecialOpeningHours == null) return NotFound("Can't find specialOpeningHours for this week");


            dbSpecialOpeningHours.MondayOpen = specialOpeningHoursDTO.MondayOpen;
            dbSpecialOpeningHours.MondayStart = CreateDateTime(specialOpeningHoursDTO.MondayStart);
            dbSpecialOpeningHours.MondayEnd = CreateDateTime(specialOpeningHoursDTO.MondayEnd);
            dbSpecialOpeningHours.TuesdayOpen = specialOpeningHoursDTO.TuesdayOpen;
            dbSpecialOpeningHours.TuesdayStart = CreateDateTime(specialOpeningHoursDTO.TuesdayStart);
            dbSpecialOpeningHours.TuesdayEnd = CreateDateTime(specialOpeningHoursDTO.TuesdayEnd);
            dbSpecialOpeningHours.WednessdayOpen = specialOpeningHoursDTO.WednessdayOpen;
            dbSpecialOpeningHours.WednessdayStart = CreateDateTime(specialOpeningHoursDTO.WednessdayStart);
            dbSpecialOpeningHours.WednessdayEnd = CreateDateTime(specialOpeningHoursDTO.WednessdayEnd);
            dbSpecialOpeningHours.ThursdayOpen = specialOpeningHoursDTO.ThursdayOpen;
            dbSpecialOpeningHours.ThursdayStart = CreateDateTime(specialOpeningHoursDTO.ThursdayStart);
            dbSpecialOpeningHours.ThursdayEnd = CreateDateTime(specialOpeningHoursDTO.ThursdayEnd);
            dbSpecialOpeningHours.FridayOpen = specialOpeningHoursDTO.FridayOpen;
            dbSpecialOpeningHours.FridayStart = CreateDateTime(specialOpeningHoursDTO.FridayStart);
            dbSpecialOpeningHours.FridayEnd = CreateDateTime(specialOpeningHoursDTO.FridayEnd);
            dbSpecialOpeningHours.SaturdayOpen = specialOpeningHoursDTO.SaturdayOpen;
            dbSpecialOpeningHours.SaturdayStart = CreateDateTime(specialOpeningHoursDTO.SaturdayStart);
            dbSpecialOpeningHours.SaturdayEnd = CreateDateTime(specialOpeningHoursDTO.SaturdayEnd);
            dbSpecialOpeningHours.SundayOpen = specialOpeningHoursDTO.SundayOpen;
            dbSpecialOpeningHours.SundayStart = CreateDateTime(specialOpeningHoursDTO.SundayStart);
            dbSpecialOpeningHours.SundayEnd = CreateDateTime(specialOpeningHoursDTO.SundayEnd);

            await context.SaveChangesAsync();

            var dtos = await context.SpecialOpeningHours.Where(x => x.EmployeeId == currentEmployeeId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return dtos;

        }

        [HttpPost("UpdateOpeningHours"), Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<OpeningHoursDTO>> UpdateOpeningHours(OpeningHoursDTO openingHoursDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentEmployeeId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            if (openingHoursDTO.EmployeeId != currentEmployeeId) return Unauthorized("Can't edit another employee's opening hours.");

            var dbOpeningHours = await context.OpeningHours.FirstOrDefaultAsync(x => x.EmployeeId == currentEmployeeId);

            dbOpeningHours.MondayOpen = openingHoursDTO.MondayOpen;
            dbOpeningHours.MondayStart = CreateDateTime(openingHoursDTO.MondayStart);
            dbOpeningHours.MondayEnd = CreateDateTime(openingHoursDTO.MondayEnd);
            dbOpeningHours.TuesdayOpen = openingHoursDTO.TuesdayOpen;
            dbOpeningHours.TuesdayStart = CreateDateTime(openingHoursDTO.TuesdayStart);
            dbOpeningHours.TuesdayEnd = CreateDateTime(openingHoursDTO.TuesdayEnd);
            dbOpeningHours.WednessdayOpen = openingHoursDTO.WednessdayOpen;
            dbOpeningHours.WednessdayStart = CreateDateTime(openingHoursDTO.WednessdayStart);
            dbOpeningHours.WednessdayEnd = CreateDateTime(openingHoursDTO.WednessdayEnd);
            dbOpeningHours.ThursdayOpen = openingHoursDTO.ThursdayOpen;
            dbOpeningHours.ThursdayStart = CreateDateTime(openingHoursDTO.ThursdayStart);
            dbOpeningHours.ThursdayEnd = CreateDateTime(openingHoursDTO.ThursdayEnd);
            dbOpeningHours.FridayOpen = openingHoursDTO.FridayOpen;
            dbOpeningHours.FridayStart = CreateDateTime(openingHoursDTO.FridayStart);
            dbOpeningHours.FridayEnd = CreateDateTime(openingHoursDTO.FridayEnd);
            dbOpeningHours.SaturdayOpen = openingHoursDTO.SaturdayOpen;
            dbOpeningHours.SaturdayStart = CreateDateTime(openingHoursDTO.SaturdayStart);
            dbOpeningHours.SaturdayEnd = CreateDateTime(openingHoursDTO.SaturdayEnd);
            dbOpeningHours.SundayOpen = openingHoursDTO.SundayOpen;
            dbOpeningHours.SundayStart = CreateDateTime(openingHoursDTO.SundayStart);
            dbOpeningHours.SundayEnd = CreateDateTime(openingHoursDTO.SundayEnd);

            await context.SaveChangesAsync();

            return openingHoursDTO;

        }


        [HttpDelete("DeleteSpecialOpeningHoursByWeek"), Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<SpecialOpeningHoursDTO>>> DeleteSpecialOpeningHoursByWeek(DateTime week)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentEmployeeId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var dbSpecialOpeningHours = await context.SpecialOpeningHours
                .FirstOrDefaultAsync(x => x.EmployeeId == currentEmployeeId 
                && x.Week.Year == week.Year 
                && ISOWeek.GetWeekOfYear(x.Week) == ISOWeek.GetWeekOfYear(week));

            if (dbSpecialOpeningHours == null) return NotFound("Can't find specialOpeningHours for this week");

            context.SpecialOpeningHours.Remove(dbSpecialOpeningHours);

            await context.SaveChangesAsync();

            var dtos = await context.SpecialOpeningHours.Where(x => x.EmployeeId == currentEmployeeId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return dtos;

        }

        /// <summary>
        /// Sets the date to 2004-1-1 (leap year) and sets the hour and minutes to the given datetime, while setting seconds to 0.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime CreateDateTime(DateTime date)
        {
            return new DateTime(2004, 1, 1, date.Hour, date.Minute, 0);
        }

    }
}
