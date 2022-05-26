using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonAPI.Models
{
    public class Service : IValidatableObject
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Salon")]
        public int SalonId { get; set; }

        public Salon Salon { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Description { get; set; }

        [Required]
        [Range(0, Double.MaxValue)]
        public double Price { get; set; }

        [Required]
        [Range(0, 500)]
        public int DurationInMinutes { get; set; }

        [Range(0, 500)]
        public int PauseStartInMinutes { get; set; }

        [Range(0, 500)]
        public int PauseEndInMinutes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PauseStartInMinutes > DurationInMinutes)
                yield return new ValidationResult("PauseStartInMinutes cannot be set after DurationInMinutes");

            if (PauseEndInMinutes > DurationInMinutes)
                yield return new ValidationResult("PauseEndInMinutes cannot be set after DurationInMinutes");

            if(PauseEndInMinutes == PauseStartInMinutes)
                yield return new ValidationResult("PauseEndInMinutes cannot be the same as PauseStartInMinutes");

            if (PauseEndInMinutes <= PauseStartInMinutes)
                yield return new ValidationResult("PauseEndInMinutes cannot be set before PauseStartInMinutes");
        }
    }
}
