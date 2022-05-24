using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models
{
    public class UserRegisterDTO
    {
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MinLength(14)]
        public string Password { get; set; } = string.Empty;
    }
}
