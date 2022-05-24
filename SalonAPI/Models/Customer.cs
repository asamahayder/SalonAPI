using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonAPI.Models
{
    public class Customer : User
    {
        [ForeignKey("Salon")]
        public int? SalonId;

        public Salon? Salon { get; set; }
    }
}
