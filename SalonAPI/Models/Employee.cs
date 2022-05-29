using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonAPI.Models
{
    [Table("Employees")]
    public class Employee : User
    {
        [ForeignKey("Salon")]
        public int? SalonId;

        public Salon? Salon { get; set; }

        [Required]
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
