

using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public abstract class User
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public Roles Role { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }
    }

    public enum Roles
    {
        Admin,
        Owner,
        Employee,
        Customer
    }

    public static class RolesExtention
    {
        public static string GetString(this Roles role)
        {
            switch (role)
            {
                case Roles.Admin: return "Admin";
                case Roles.Owner: return "Owner";
                case Roles.Employee: return "Employee";
                case Roles.Customer: return "Customer";
                default: return "Admin";
            }
        }
    }

}
