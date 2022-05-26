using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models
{
    public class Owner : User
    {
        [Required]
        [MaxLength(50)]
        public string SalonChainName { get; set; }
    }
}
