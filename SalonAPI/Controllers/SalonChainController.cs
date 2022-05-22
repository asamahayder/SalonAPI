using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalonAPI.Models;

namespace SalonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalonChainController : ControllerBase
    {
        private readonly DataContext context;

        public SalonChainController(DataContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<SalonChain>>> Get()
        {
            return Ok(await context.SalonChains.ToListAsync());
        }

        [HttpGet("id")]
        public async Task<ActionResult<SalonChain>> Get(int Id)
        {
            var salonChain =  await context.SalonChains.FindAsync(Id);
            if (salonChain == null)
            {
                return BadRequest("SalonChain not found");
            }
            else
            {
                return Ok(salonChain);
            }
            
        }

        [HttpPost]
        public async Task<ActionResult<List<SalonChain>>> AddSalonChain(SalonChain salonChain)
        {
            context.SalonChains.Add(salonChain);
            await context.SaveChangesAsync();

            return Ok(await context.SalonChains.ToListAsync());
        }

        [HttpPut]
        public async Task<ActionResult<SalonChain>> UpddateSalonChain(SalonChain request)
        {
            var salonChain = await context.SalonChains.FindAsync(request.Id);
            if (salonChain == null)
            {
                return BadRequest("SalonChain not found");
            }
            else
            {
                salonChain.OwnerId = request.OwnerId;
                salonChain.Name = request.Name;
                await context.SaveChangesAsync();
                return Ok(await context.SalonChains.ToListAsync());
            }
        }

        [HttpDelete("id")]
        public async Task<ActionResult<List<SalonChain>>> Delete(int Id)
        {
            var salonChain = await context.SalonChains.FindAsync(Id);
            if (salonChain == null)
            {
                return BadRequest("SalonChain not found");
            }
            else
            {
                context.SalonChains.Remove(salonChain);
                await context.SaveChangesAsync();
                return Ok(await context.SalonChains.ToListAsync());
            }

        }
    }
}
