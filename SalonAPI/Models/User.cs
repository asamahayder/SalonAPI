

using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models
{
    public class User
    {
        [Required]
        public string Username { get; set; }  = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public Role Role { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }
    }

    public enum Role
    {
        Admin,
        Owner,
        Customer
    }
}
