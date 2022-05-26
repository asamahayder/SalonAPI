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
        public async Task<ActionResult<List<Salon>>> Get()
        {
            return Ok(await context.Salons.ToListAsync());
        }

        [HttpGet("id")]
        public async Task<ActionResult<Salon>> Get(int Id)
        {
            var salon = await context.Salons.FindAsync(Id);
            if (salon == null)
            {
                return BadRequest("Salon not found");
            }
            
            return Ok(salon);
            

        }

        [HttpPost("CreateSalon"), Authorize(Roles ="Admin,Owner")]
        public async Task<ActionResult<List<Salon>>> CreateSalon(SalonDTO salonDTO)
        {
            //Getting user identity
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var owner = await context.Owners.AsNoTracking().FirstOrDefaultAsync(x => x.Email == ownerEmail);

            var salon = new Salon()
            {
                OwnerId = owner.Id,
                City = salonDTO.City,
                PostCode = salonDTO.PostCode,
                StreetName = salonDTO.StreetName,
                StreetNumber = salonDTO.StreetNumber,
                Suit = salonDTO.Suit,
                Door = salonDTO.Door,
                Phone = salonDTO.Phone,
                Email = salonDTO.Email
            };
            context.Salons.Add(salon);
            await context.SaveChangesAsync();

            return Ok(await context.Salons.ToListAsync());
        }

        [HttpPut("UpdateSalon"), Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<Salon>> UpdateSalon(SalonDTO salonDTO)
        {
            var dbSalon = await context.Salons.FindAsync(salonDTO.Id);
            if (dbSalon == null)
            {
                return BadRequest("Salon not found");
            }

            //Getting user identity and checking if salon owner and logged in owner is the same.
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value.ToString();
            var owner = await context.Owners.AsNoTracking().FirstOrDefaultAsync(x => x.Email == ownerEmail);

            if(dbSalon.OwnerId != owner.Id)
            {
                return Unauthorized("Authorized user does not have permission to edit this salon.");
            }

            //Updating
            dbSalon.City = salonDTO.City;
            dbSalon.PostCode = salonDTO.PostCode;
            dbSalon.StreetName = salonDTO.StreetName;
            dbSalon.StreetNumber = salonDTO.StreetNumber;
            dbSalon.Suit = salonDTO.Suit;
            dbSalon.Door = salonDTO.Door;
            dbSalon.Phone = salonDTO.Phone;
            dbSalon.Email = salonDTO.Email;

            await context.SaveChangesAsync();
            return Ok(await context.Salons.ToListAsync());
        }

        [HttpDelete("DeleteSalon"), Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<Salon>>> Delete(int Id)
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
            return Ok(await context.Salons.ToListAsync());
            

        }
    }
}
