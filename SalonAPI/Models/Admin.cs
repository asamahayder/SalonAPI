using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models
{
    public class Admin : User
    {
        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }
    }
}
