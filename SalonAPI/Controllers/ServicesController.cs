using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalonAPI.Models;
using SalonAPI.Models.DTOs;
using System.Security.Claims;

namespace SalonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly DataContext context;

        public ServicesController(DataContext context)
        {
            this.context = context;
        }

        //TODO when getting a service, include the employees that it has assigned. But when updating/creating a service, do NOT include employees.

        [HttpGet("ServicesBySalon")]
        public async Task<ActionResult<List<ServiceDTO>>> GetServicesBySalon(int salonId)
        {
            var services = await context.Services.Include(x => x.Employees).Where(x => x.SalonId == salonId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();
            return Ok(services);
        }

        [HttpGet("ServicesByEmployee")]
        public async Task<ActionResult<List<ServiceDTO>>> GetServicesByEmployee(int employeeId)
        {
            //return Ok(await context.Employees.Where(x => x.Id == employeeId).SelectMany(x => x.Services).ToListAsync());

            var services = await context.Employees.Where(x => x.Id == employeeId)
                .SelectMany(x => x.Services).Include(x => x.Employees).Select(x => Mapper.MapToDTO(x)).ToListAsync();
            return Ok(services);
        }


        [HttpGet("id")]
        public async Task<ActionResult<ServiceDTO>> Get(int Id)
        {
            var service = await context.Services.Include(x => x.Employees).Where(x => x.Id == Id).Select(x => Mapper.MapToDTO(x)).FirstOrDefaultAsync();
            if (service == null)
            {
                return BadRequest("Service not found");
            }

            return Ok(service);
        }


        [HttpPost("CreateService"), Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<ServiceDTO>>> CreateService(ServiceDTO serviceDTO)
        {
            //Getting user identity
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var owner = await context.Owners.AsNoTracking().FirstOrDefaultAsync(x => x.Email == ownerEmail);
            

            var salon = await context.Salons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == serviceDTO.SalonId);
            if (salon == null) return BadRequest("Salon was not found");
            if (salon.OwnerId != owner.Id) return Unauthorized("Logged in user does not have permission to create a service for this salon.");

            var serviceDtoEmployees = await context.Employees.Where(x => serviceDTO.EmployeesIds.Contains(x.Id)).ToListAsync();

            foreach (var serviceDtoEmployee in serviceDtoEmployees)
            {
                var employeeSalonId = serviceDtoEmployee.SalonId;

                if (employeeSalonId == null || employeeSalonId != salon.Id)
                    return Unauthorized("Employee not part of salon");
            }

            var service = new Service()
            {
                SalonId = serviceDTO.SalonId,
                Name = serviceDTO.Name,
                Description = serviceDTO.Description,
                Price = serviceDTO.Price,
                DurationInMinutes = serviceDTO.DurationInMinutes,
                PauseStartInMinutes = serviceDTO.PauseStartInMinutes,
                PauseEndInMinutes = serviceDTO.PauseEndInMinutes,
                Employees = serviceDtoEmployees
            };

            context.Services.Add(service);
            await context.SaveChangesAsync();

            var dtos = await context.Services.Include(x => x.Employees).Where(x => x.SalonId == salon.Id)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();
            return Ok(dtos);
        }


        [HttpPut("UpdateService"), Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<ServiceDTO>>> UpdateService(ServiceDTO serviceDTO)
        {
            var dbService = await context.Services.Include(x => x.Employees).FirstOrDefaultAsync(x => x.Id == serviceDTO.Id);
            if (dbService == null)
            {
                return BadRequest("Service not found");
            }

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var owner = await context.Owners.AsNoTracking().FirstOrDefaultAsync(x => x.Email == ownerEmail);

            var salon = await context.Salons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == serviceDTO.SalonId);
            if (salon == null) return BadRequest("Salon was not found");
            if (salon.OwnerId != owner.Id) return Unauthorized("Logged in user does not have permission to edit a service for this salon.");

            var serviceDtoEmployees = await context.Employees.Where(x => serviceDTO.EmployeesIds.Contains(x.Id)).ToListAsync();

            foreach (var serviceDtoEmployee in serviceDtoEmployees)
            {
                var employeeSalonId = serviceDtoEmployee.SalonId;

                if (employeeSalonId == null || employeeSalonId != salon.Id)
                    return Unauthorized("Employee not part of salon");
            }

            //Updating
            dbService.Name = serviceDTO.Name;
            dbService.Description = serviceDTO.Description;
            dbService.Price = serviceDTO.Price;
            dbService.DurationInMinutes = serviceDTO.DurationInMinutes;
            dbService.PauseStartInMinutes = serviceDTO.PauseStartInMinutes;
            dbService.PauseEndInMinutes = serviceDTO.PauseEndInMinutes;
            dbService.Employees = serviceDtoEmployees;

            await context.SaveChangesAsync();

            var dtos = await context.Services.Include(x => x.Employees).Where(x => x.SalonId == salon.Id)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(dtos);
        }


        [HttpDelete("DeleteService"), Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<ServiceDTO>>> Delete(int Id)
        {
            var dbService = await context.Services.FindAsync(Id);
            if (dbService == null)
            {
                return BadRequest("Service not found");
            }

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var owner = await context.Owners.AsNoTracking().FirstOrDefaultAsync(x => x.Email == ownerEmail);

            var salon = await context.Salons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dbService.SalonId);
            if (salon == null) return BadRequest("Salon was not found");
            if (salon.OwnerId != owner.Id) return Unauthorized("Logged in user does not have permission to delete a service from this salon.");

            context.Services.Remove(dbService);
            await context.SaveChangesAsync();
            
            var dtos = await context.Services.Include(x => x.Employees).Where(x => x.SalonId == salon.Id)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();
            return Ok(dtos);
        }
    }
}
