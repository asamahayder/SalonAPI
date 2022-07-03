using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonAPI.Models
{
    public class Booking
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int BookedById { get; set; }

        public User User { get; set; }


        [Required]
        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }

        public Customer? Customer { get; set; }


        [Required]
        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        [Required]
        [ForeignKey("Service")]
        public int ServiceId { get; set; }

        public Service Service { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        [MaxLength(300)]
        public string? Note { get; set; }


    }
}
