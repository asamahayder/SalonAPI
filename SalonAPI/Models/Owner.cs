using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonAPI.Models
{
    [Table("Owners")]
    public class Owner : User
    {
        [Required]
        [MaxLength(50)]
        public string SalonChainName { get; set; } = string.Empty;
    }
}
