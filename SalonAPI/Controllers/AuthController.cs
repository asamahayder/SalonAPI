using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SalonAPI.Models;
using SalonAPI.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SalonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly DataContext context;

        public AuthController(IConfiguration configuration, DataContext context)
        {
            this.configuration = configuration;
            this.context = context;
        }

        [HttpPost("RegisterOwner")]
        public async Task<ActionResult<User>> RegisterOwner(UserRegisterDTO userDTO)
        {
            //checking if a user with same email already exists
            var dbUser = await context.Users.FirstOrDefaultAsync(x => x.Email == userDTO.Email);
            if (dbUser != null) return BadRequest("A User with this email already exists.");

            //creating and saving the new user
            CreatePasswordHash(userDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var newOwner = new Owner() { 
                Email = userDTO.Email, 
                FirstName = userDTO.FirstName.Trim(),
                LastName = userDTO.LastName.Trim(),
                Phone = userDTO.Phone,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            context.Owners.Add(newOwner);
            await context.SaveChangesAsync();

            return Ok("User registered succesfully");
        }

        [HttpPost("RegisterEmployee")]
        public async Task<ActionResult<User>> RegisterEmployee(UserRegisterDTO userDTO)
        {
            //checking if a user with same email already exists
            var dbUser = await context.Users.FirstOrDefaultAsync(x => x.Email == userDTO.Email);
            if (dbUser != null) return BadRequest("A User with this email already exists.");

            //creating and saving the new user
            CreatePasswordHash(userDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var newEmployee = new Employee()
            {
                Email = userDTO.Email,
                FirstName = userDTO.FirstName.Trim(),
                LastName = userDTO.LastName.Trim(),
                Phone = userDTO.Phone,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            context.Employees.Add(newEmployee);
            await context.SaveChangesAsync();

            return Ok("User registered succesfully");
        }

        [HttpPost("RegisterCustomer")]
        public async Task<ActionResult<User>> RegisterCustomer(UserRegisterDTO userDTO)
        {
            //checking if a user with same email already exists
            var dbUser = await context.Users.FirstOrDefaultAsync(x => x.Email == userDTO.Email);
            if (dbUser != null) return BadRequest("A User with this email already exists.");

            //creating and saving the new user
            CreatePasswordHash(userDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var newCustomer = new Customer()
            {
                Email = userDTO.Email,
                FirstName = userDTO.FirstName.Trim(),
                LastName = userDTO.LastName.Trim(),
                Phone = userDTO.Phone,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            context.Customers.Add(newCustomer);
            await context.SaveChangesAsync();

            return Ok("User registered succesfully");
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDTO userDTO)
        {
            //checking if user with email exists
            var dbUser = await context.Users.FirstOrDefaultAsync(x => x.Email == userDTO.Email);
            if (dbUser == null) return BadRequest("No User with this email exists.");
            

            if (!VerifyPasswordHash(userDTO.Password, dbUser.PasswordHash, dbUser.PasswordSalt))
            {
                return BadRequest("Wrong password");
            }

            var token = CreateToken(dbUser);

            return Ok(token);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                
            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                passwordSalt = hmac.Key;
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);

            }
        }

        
    }
}
