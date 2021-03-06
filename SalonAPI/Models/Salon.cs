using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonAPI.Models
{
    public class Salon
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Owner")]
        public int OwnerId { get; set; }

        public Owner Owner { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.PostalCode)]
        public string PostCode { get; set; } = string.Empty;

        [Required]
        public string StreetName { get; set; } = string.Empty;

        [MaxLength(10)]
        [Required]
        public string StreetNumber { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Suit { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Door { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; } = string.Empty;

        public List<Employee> Employees { get; set; }

    }
}
