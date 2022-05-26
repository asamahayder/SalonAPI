using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models.DTOs
{
    public class SalonDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.PostalCode)]
        public string PostCode { get; set; } = string.Empty;

        [Required]
        public string StreetName { get; set; } = string.Empty;

        [Required]
        public string StreetNumber { get; set; } = string.Empty;

        public string? Suit { get; set; } = string.Empty;

        public string? Door { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; } = string.Empty;
    }
}
