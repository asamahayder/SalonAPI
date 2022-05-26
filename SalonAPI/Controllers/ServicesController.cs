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


        [HttpGet]
        public async Task<ActionResult<List<Service>>> Get()
        {
            return Ok(await context.Services.ToListAsync());
        }


        [HttpGet("id")]
        public async Task<ActionResult<Service>> Get(int Id)
        {
            var service = await context.Services.FindAsync(Id);
            if (service == null)
            {
                return BadRequest("Service not found");
            }

            return Ok(service);
        }


        [HttpPost("CreateService"), Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<Service>>> CreateService(ServiceDTO serviceDTO)
        {
            //Getting user identity
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var owner = await context.Owners.AsNoTracking().FirstOrDefaultAsync(x => x.Email == ownerEmail);
            

            var salon = await context.Salons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == serviceDTO.SalonId);
            if (salon == null) return BadRequest("Salon was not found");
            if (salon.OwnerId != owner.Id) return Unauthorized("Logged in user does not have permission to create a service for this salon.");


            var service = new Service()
            {
                SalonId = serviceDTO.SalonId,
                Name = serviceDTO.Name,
                Description = serviceDTO.Description,
                Price = serviceDTO.Price,
                DurationInMinutes = serviceDTO.DurationInMinutes,
                PauseStartInMinutes = serviceDTO.PauseStartInMinutes,
                PauseEndInMinutes = serviceDTO.PauseEndInMinutes
            };

            context.Services.Add(service);
            await context.SaveChangesAsync();

            return Ok(await context.Services.ToListAsync());
        }


        [HttpPut("UpdateService"), Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<Service>> UpdateService(ServiceDTO serviceDTO)
        {
            var dbService = await context.Services.FindAsync(serviceDTO.Id);
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

            //Updating
            dbService.Name = serviceDTO.Name;
            dbService.Description = serviceDTO.Description;
            dbService.Price = serviceDTO.Price;
            dbService.DurationInMinutes = serviceDTO.DurationInMinutes;
            dbService.PauseStartInMinutes = serviceDTO.PauseStartInMinutes;
            dbService.PauseEndInMinutes = serviceDTO.PauseEndInMinutes;

            await context.SaveChangesAsync();
            return Ok(await context.Services.ToListAsync());
        }


        [HttpDelete("DeleteService"), Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<Service>>> Delete(int Id)
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
            return Ok(await context.Services.ToListAsync());
        }
    }
}
