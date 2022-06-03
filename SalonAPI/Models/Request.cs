using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonAPI.Models
{
    public class Request
    {

        [Required]
        public int Id { get; set; }
        
        [Required]
        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        [Required]
        [ForeignKey("Salon")]
        public int SalonId { get; set; }

        public Salon Salon { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        public RequestStatus RequestStatus { get; set; }

    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Denied
    }

    public static class RequestStatusExtention
    {
        public static string GetString(this RequestStatus requestStatus)
        {
            switch (requestStatus)
            {
                case RequestStatus.Pending: return "Pending";
                case RequestStatus.Approved: return "Approved";
                case RequestStatus.Denied: return "Denied";
                default: return "Pending";
            }
        }
    }
}
