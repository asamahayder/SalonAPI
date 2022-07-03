using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models.DTOs
{
    public class BookingDTO
    {

        [Required]
        public int Id { get; set; }

        [Required]
        public int BookedById { get; set; }

        [Required]
        public int? CustomerId { get; set; }


        [Required]
        public int EmployeeId { get; set; }


        [Required]
        public int ServiceId { get; set; }


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
