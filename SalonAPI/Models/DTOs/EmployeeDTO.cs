using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models.DTOs
{
    public class EmployeeDTO
    {
        
        [Required]
        public int Id { get; set; }

        
        public int? SalonId { get; set; }

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
        public string Role { get; set; } = string.Empty;
    }
}
