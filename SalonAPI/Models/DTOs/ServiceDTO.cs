using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models.DTOs
{
    public class ServiceDTO : IValidatableObject
    {
        
        [Required]
        public int Id { get; set; }

        [Required]
        public int SalonId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Description { get; set; }

        [Required]
        [Range(0, Double.MaxValue)]
        public double Price { get; set; }

        [Required]
        [Range(1, 500)]
        public int DurationInMinutes { get; set; }

        [Range(0, 500)]
        public int? PauseStartInMinutes { get; set; }

        [Range(0, 500)]
        public int? PauseEndInMinutes { get; set; }

        [Required]
        public ICollection<int> EmployeesIds { get; set; } = new List<int>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((PauseStartInMinutes != null && PauseEndInMinutes == null) || (PauseStartInMinutes == null && PauseEndInMinutes != null))
                yield return new ValidationResult("Not possible to only specify one of the pause durations. Either none of them are specified, or both are specified.");

            if (PauseEndInMinutes != null && PauseStartInMinutes != null)
            {
                if (PauseStartInMinutes > DurationInMinutes)
                    yield return new ValidationResult("PauseStartInMinutes cannot be set after DurationInMinutes");

                if (PauseEndInMinutes > DurationInMinutes)
                    yield return new ValidationResult("PauseEndInMinutes cannot be set after DurationInMinutes");

                if (PauseEndInMinutes == PauseStartInMinutes)
                    yield return new ValidationResult("PauseEndInMinutes cannot be the same as PauseStartInMinutes");

                if (PauseEndInMinutes < PauseStartInMinutes)
                    yield return new ValidationResult("PauseEndInMinutes cannot be set before PauseStartInMinutes");

                if (PauseStartInMinutes == 0)
                    yield return new ValidationResult("PauseStartInMinutes cannot be set to 0");

                if (PauseEndInMinutes == DurationInMinutes)
                    yield return new ValidationResult("PauseEndInMinutes cannot be set to DurationInMinutes");

            }
        }

    }
}
