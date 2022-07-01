using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models
{
    public class LogEntry
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public LogCategory LogCategory { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

    }

    public enum LogCategory
    {
        info,
        warning,
        error
    }
}
