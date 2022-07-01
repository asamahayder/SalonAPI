using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models.DTOs
{
    public class AuthResponseDTO
    {
        [Required]
        public string Message { get; set; }

        public int? UserId { get; set; }
    }
}
