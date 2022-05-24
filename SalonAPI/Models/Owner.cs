using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models
{
    public class Owner : User
    {
        [Required]
        [MaxLength(50)]
        public string SalonName { get; set; } = string.Empty;

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }
    }
}
