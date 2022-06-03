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
    [Authorize]
    public class RequestController : ControllerBase
    {
        private readonly DataContext context;

        public RequestController(DataContext context)
        {
            this.context = context;
        }

        [HttpGet("GetRequestsBySalonId")]
        [Authorize(Roles ="Admin,Owner")]
        public async Task<ActionResult<List<RequestDTO>>> GetRequestsBySalonId(int salonId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            //can only get his own salons
            var salon = await context.Salons.FirstOrDefaultAsync(x => x.Id == salonId);
            if (salon == null) return NotFound("Salon not found");
            if (salon.OwnerId != ownerId) return Unauthorized("No permission to access this salon");

            var requests = await context.Requests.Where(x => x.SalonId == salonId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();
            return Ok(requests);
        }

        [HttpGet("GetRequestsByEmployee")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<RequestDTO>>> GetRequestsByEmployee()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentEmployeeId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var requests = await context.Requests.Where(x => x.EmployeeId == currentEmployeeId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(requests);
        }

        [HttpGet("GetRequests")]
        [Authorize(Roles="Admin")]
        public async Task<ActionResult<List<RequestDTO>>> GetRequests()
        {
            var requests = await context.Requests.Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(requests);
        }


        [HttpPost("CreateRequest")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<RequestDTO>>> CreateRequest(int salonId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentEmployeeId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var employee = await context.Employees.Where(x => x.Id == currentEmployeeId).FirstOrDefaultAsync();
            if (employee.SalonId != null) return BadRequest("Can't create a new request when already assigned to a salon");

            var salon = await context.Salons.Where(x => x.Id == salonId).FirstOrDefaultAsync();
            if (salon == null) return NotFound("salon not found");

            var existingRequests = await context.Requests.Where(x => x.EmployeeId == currentEmployeeId).ToListAsync();
            foreach (var existingRequest in existingRequests)
            {
                if (existingRequest.RequestStatus == RequestStatus.Pending || employee.SalonId != null) 
                    return BadRequest("Can't create a new request while another request is pending");
            }

            var newRequest = new Request()
            {
                SalonId = salonId,
                Salon = salon,
                EmployeeId = currentEmployeeId,
                Employee = employee,
                Date = DateTime.Now,
                RequestStatus = RequestStatus.Pending
            };

            context.Requests.Add(newRequest);

            await context.SaveChangesAsync();

            var requests = await context.Requests.Where(x => x.EmployeeId == currentEmployeeId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(requests);
        }

        [HttpPut("ApproveRequest")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<RequestDTO>>> ApproveRequest(int requestID)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var request = await context.Requests.Where(x => x.Id == requestID).FirstOrDefaultAsync();
            if (request == null) return NotFound("request not found");

            var salon = await context.Salons.Where(x => x.Id == request.SalonId).FirstOrDefaultAsync();

            var employee = await context.Employees.Where(x => x.Id == request.EmployeeId).FirstOrDefaultAsync();

            if (salon.OwnerId != ownerId) return Unauthorized("No permission to access this salon");

            if (request.RequestStatus != RequestStatus.Pending) return BadRequest("Can't approve a request that is not pending");

            request.RequestStatus = RequestStatus.Approved;
            employee.SalonId = salon.Id;
            employee.Salon = salon;


            await context.SaveChangesAsync();

            var requests = await context.Requests.Where(x => x.SalonId == salon.Id)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(requests);
        }

        [HttpPut("DenyRequest")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<List<RequestDTO>>> DenyRequest(int requestID)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var ownerId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var request = await context.Requests.Where(x => x.Id == requestID).FirstOrDefaultAsync();
            if (request == null) return NotFound("request not found");

            var salon = await context.Salons.Where(x => x.Id == request.SalonId).FirstOrDefaultAsync();

            if (salon.OwnerId != ownerId) return Unauthorized("No permission to access this salon");

            if (request.RequestStatus != RequestStatus.Pending) return BadRequest("Can't deny a request that is not pending");

            request.RequestStatus = RequestStatus.Denied;

            await context.SaveChangesAsync();

            var requests = await context.Requests.Where(x => x.SalonId == salon.Id)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(requests);
        }

        [HttpDelete("DeleteRequest"), Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<RequestDTO>>> Delete(int requestId)
        {
            //can only delete request which are pending
            //can only delete your own requests

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentEmployeeId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var request = await context.Requests.FirstOrDefaultAsync(x => x.Id == requestId);

            if (request == null) return NotFound("request not found");

            if (request.EmployeeId != currentEmployeeId) return Unauthorized("No permission to delete this request");

            if (request.RequestStatus != RequestStatus.Pending) return BadRequest("Can't delete a request which is not pending");

            context.Requests.Remove(request);
            await context.SaveChangesAsync();

            var requests = await context.Requests.Where(x => x.EmployeeId == currentEmployeeId)
                .Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(requests);

        }



    }
}
