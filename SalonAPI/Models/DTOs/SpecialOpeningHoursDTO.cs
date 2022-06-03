using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models.DTOs
{
    public class SpecialOpeningHoursDTO : IValidatableObject
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Week { get; set; }


        [Required]
        public bool MondayOpen { get; set; } = true;


        [Required]
        [DataType(DataType.DateTime)]
        public DateTime MondayStart { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime MondayEnd { get; set; }

        [Required]
        public bool TuesdayOpen { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TuesdayStart { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TuesdayEnd { get; set; }


        [Required]
        public bool WednessdayOpen { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime WednessdayStart { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime WednessdayEnd { get; set; }


        [Required]
        public bool ThursdayOpen { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ThursdayStart { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ThursdayEnd { get; set; }


        [Required]
        public bool FridayOpen { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime FridayStart { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime FridayEnd { get; set; }


        [Required]
        public bool SaturdayOpen { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SaturdayStart { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SaturdayEnd { get; set; }


        [Required]
        public bool SundayOpen { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SundayStart { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SundayEnd { get; set; }

        

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MondayOpen && MondayStart.CompareTo(MondayEnd) > -1)
                yield return new ValidationResult("MondayStart cannot be at the same or later time than MondayEnd");

            if (TuesdayOpen && TuesdayStart.CompareTo(TuesdayEnd) > -1)
                yield return new ValidationResult("TuesdayStart cannot be at the same or later time than TuesdayEnd");

            if (WednessdayOpen && WednessdayStart.CompareTo(WednessdayEnd) > -1)
                yield return new ValidationResult("WednessdayStart cannot be at the same or later time than WednessdayEnd");

            if (ThursdayOpen && ThursdayStart.CompareTo(ThursdayEnd) > -1)
                yield return new ValidationResult("ThursdayStart cannot be at the same or later time than ThursdayEnd");

            if (FridayOpen && FridayStart.CompareTo(FridayEnd) > -1)
                yield return new ValidationResult("FridayStart cannot be at the same or later time than FridayEnd");

            if (SaturdayOpen && SaturdayStart.CompareTo(SaturdayEnd) > -1)
                yield return new ValidationResult("SaturdayStart cannot be at the same or later time than SaturdayEnd");

            if (SundayOpen && SundayStart.CompareTo(SundayEnd) > -1)
                yield return new ValidationResult("SundayStart cannot be at the same or later time than SundayEnd");


        }
    }
}
