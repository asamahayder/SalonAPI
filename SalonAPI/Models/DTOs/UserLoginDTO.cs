using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models.DTOs
{
    public class UserLoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
