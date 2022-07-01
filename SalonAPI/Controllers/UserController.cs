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
    public class UserController : ControllerBase
    {
        private readonly DataContext context;

        public UserController(DataContext context)
        {
            this.context = context;

        }

        [HttpGet("GetUser")]
        public async Task<ActionResult<UserDTO>> GetUser()
        {
            //user can only get his own information
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentUserId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var currentUser = await context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);

            if (currentUser == null) return NotFound("User not found");

            return Ok(Mapper.MapToDTO(currentUser));
        }

        [HttpGet("GetUserById"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDTO>> GetUserById(int id)
        {
            
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null) return NotFound("User not found");

            return Ok(Mapper.MapToDTO(user));
        }

        [HttpGet("GetUserByBookingId"), Authorize(Roles = "Admin,Owner,Employee")]
        public async Task<ActionResult<UserDTO>> GetUserByBookingId(int bookingId)
        {
            //Owners and employees can only get user information through their own bookings. 
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentUserId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var currentUser = await context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
            if (currentUser == null) return NotFound("User not found");

            //does current user have access to this booking?

            var booking = await context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId);
            if (booking == null) return NotFound("Booking not found");

            if (currentUser.Role == Roles.Employee && booking.EmployeeId != currentUser.Id)
                return Unauthorized("No permission to access this booking");

            
            if(currentUser.Role == Roles.Owner)
            {
                var employee = await context.Employees.Include(x => x.Salon)
                    .FirstOrDefaultAsync(x => x.Id == booking.EmployeeId);

                if (employee.Salon.OwnerId != currentUser.Id) 
                    return Unauthorized("No permission to access this booking");
            }

            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == booking.BookedById);

            return Ok(Mapper.MapToDTO(user));
        }

        [HttpPut("UpdateUser")]
        public async Task<ActionResult<UserDTO>> UpdateUser(UserDTO userDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentUserId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var currentUser = await context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
            if (currentUser == null) return NotFound("User not found");

            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userDTO.Id);

            if (currentUser.Id != user.Id) return Unauthorized("Can't edit this user");

            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.Phone = userDTO.Phone;

            await context.SaveChangesAsync();

            return Ok(Mapper.MapToDTO(user));
        }

        [HttpDelete("DeleteUser")]
        public async Task<ActionResult<UserDTO>> DeleteUser(int userId)
        {
            //Owners and employees can only get user information through their own bookings. 
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized("Identity is null");
            var currentUserId = Int32.Parse(identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value.ToString());

            var currentUser = await context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
            if (currentUser == null) return NotFound("User not found");

            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return NotFound("User was not found");

            if (currentUser.Id != user.Id) return Unauthorized("Can't edit this user");

            context.Users.Remove(user);

            await context.SaveChangesAsync();

            return Ok(Mapper.MapToDTO(user));
        }




    }
}
