using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonAPI.Data;
using SalonAPI.Models;
using SalonAPI.Models.DTOs;

namespace SalonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalonController : ControllerBase
    {
        private readonly DataContext context;

        public SalonController(DataContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<SalonDTO>>> Get()
        {
            var salons = await context.Salons.Include(x => x.Employees).Select(x => Mapper.MapToDTO(x)).ToListAsync();
            return Ok(salons);
        }

        [HttpGet("id")]
        public async Task<ActionResult<SalonDTO>> Get(int Id)
        {
            var salon = await context.Salons.Include(x => x.Employees).Where(x => x.Id == Id)
                .Select(x => Mapper.MapToDTO(x)).FirstOrDefaultAsync();
            
            if (salon == null)
            {
                return BadRequest("Salon not found");
            }
            
            return Ok(salon);
        }

        [HttpGet("GetByOwnerId")]
        public async Task<ActionResult<List<SalonDTO>>> GetByOwnerId(int OwnerId)
        {
            var salons = await context.Salons.Include(x => x.Employees).Where(x => x.OwnerId == OwnerId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(salons);
        }

        [HttpPost("CreateSalon"), Authorize(Roles ="Admin,Owner")]
        public async Task<ActionResult<List<SalonDTO>>> CreateSalon(SalonDTO salonDTO)
        {
            //Getting user identity
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var owner = await context.Owners.AsNoTracking().FirstOrDefaultAsync(x => x.Email == ownerEmail);

            var employees = await context.Employees
                .Where(x => x.Role == Roles.Employee && salonDTO.EmployeesIds.Contains(x.Id))
                .ToListAsync();


            var salon = new Salon()
            {
                OwnerId = owner.Id,
                Name = salonDTO.Name,
                City = salonDTO.City,
                PostCode = salonDTO.PostCode,
                StreetName = salonDTO.StreetName,
                StreetNumber = salonDTO.StreetNumber,
                Suit = salonDTO.Suit,
                Door = salonDTO.Door,
                Phone = salonDTO.Phone,
                Email = salonDTO.Email,
                Employees = employees
            };

            context.Salons.Add(salon);
            await context.SaveChangesAsync();

            var salons = await context.Salons.Include(x => x.Employees).Where(x => x.OwnerId == owner.Id)
               .Select(x => Mapper.MapToDTO(x)).ToListAsync();
            return Ok(salons);
        }

        [HttpPut("UpdateSalon"), Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<SalonDTO>>> UpdateSalon(SalonDTO salonDTO)
        {
            var dbSalon = await context.Salons.Include(x => x.Employees).FirstOrDefaultAsync(x => x.Id == salonDTO.Id);
            if (dbSalon == null)
            {
                return BadRequest("Salon not found");
            }

            //Getting user identity and checking if salon owner and logged in owner is the same.
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var owner = await context.Owners.AsNoTracking().FirstOrDefaultAsync(x => x.Email == ownerEmail);

            


            if (dbSalon.OwnerId != owner.Id)
            {
                return Unauthorized("Authorized user does not have permission to edit this salon.");
            }

            //Updating
            dbSalon.Name = salonDTO.Name;
            dbSalon.City = salonDTO.City;
            dbSalon.PostCode = salonDTO.PostCode;
            dbSalon.StreetName = salonDTO.StreetName;
            dbSalon.StreetNumber = salonDTO.StreetNumber;
            dbSalon.Suit = salonDTO.Suit;
            dbSalon.Door = salonDTO.Door;
            dbSalon.Phone = salonDTO.Phone;
            dbSalon.Email = salonDTO.Email;


            dbSalon.Employees.RemoveAll(x => !salonDTO.EmployeesIds.Contains(x.Id));

            await context.SaveChangesAsync();

            var salons = await context.Salons.Include(x => x.Employees).Where(x => x.OwnerId == owner.Id)
               .Select(x => Mapper.MapToDTO(x)).ToListAsync();
            return Ok(salons);
        }

        [HttpDelete("DeleteSalon"), Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<SalonDTO>>> Delete(int Id)
        {
            var dbSalon = await context.Salons.FindAsync(Id);
            if (dbSalon == null)
            {
                return BadRequest("Salon not found");
            }

            //Getting user identity and checking if salon owner and logged in owner is the same.
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var owner = await context.Owners.AsNoTracking().FirstOrDefaultAsync(x => x.Email == ownerEmail);

            if(owner.Id != dbSalon.OwnerId) return Unauthorized("Authorized user does not have permission to edit this salon.");

            context.Salons.Remove(dbSalon);
            await context.SaveChangesAsync();

            var salons = await context.Salons.Include(x => x.Employees).Where(x => x.OwnerId == owner.Id)
               .Select(x => Mapper.MapToDTO(x)).ToListAsync();
            return Ok(salons);


        }
    }
}
