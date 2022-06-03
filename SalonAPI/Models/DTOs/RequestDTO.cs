using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models.DTOs
{
    public class RequestDTO
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int SalonId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public RequestStatus RequestStatus { get; set; }
    }
}
