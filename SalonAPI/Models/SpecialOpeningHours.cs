using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonAPI.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    
    public class SpecialOpeningHours : IValidatableObject
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Week { get; set; }

        [Required]
        public bool MondayOpen { get; set; } = true;


        [Required]
        [DataType(DataType.DateTime)]
        public DateTime MondayStart { get; set; } = new DateTime(2004, 1, 1, 9, 0, 0);

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime MondayEnd { get; set; } = new DateTime(2004, 1, 1, 17, 0, 0);

        [Required]
        public bool TuesdayOpen { get; set; } = true;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TuesdayStart { get; set; } = new DateTime(2004, 1, 1, 9, 0, 0);

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TuesdayEnd { get; set; } = new DateTime(2004, 1, 1, 17, 0, 0);


        [Required]
        public bool WednessdayOpen { get; set; } = true;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime WednessdayStart { get; set; } = new DateTime(2004, 1, 1, 9, 0, 0);

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime WednessdayEnd { get; set; } = new DateTime(2004, 1, 1, 17, 0, 0);


        [Required]
        public bool ThursdayOpen { get; set; } = true;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ThursdayStart { get; set; } = new DateTime(2004, 1, 1, 9, 0, 0);

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ThursdayEnd { get; set; } = new DateTime(2004, 1, 1, 17, 0, 0);


        [Required]
        public bool FridayOpen { get; set; } = true;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime FridayStart { get; set; } = new DateTime(2004, 1, 1, 9, 0, 0);

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime FridayEnd { get; set; } = new DateTime(2004, 1, 1, 17, 0, 0);


        [Required]
        public bool SaturdayOpen { get; set; } = true;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SaturdayStart { get; set; } = new DateTime(2004, 1, 1, 9, 0, 0);

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SaturdayEnd { get; set; } = new DateTime(2004, 1, 1, 17, 0, 0);


        [Required]
        public bool SundayOpen { get; set; } = true;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SundayStart { get; set; } = new DateTime(2004, 1, 1, 9, 0, 0);

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SundayEnd { get; set; } = new DateTime(2004, 1, 1, 17, 0, 0);


        /// <summary>
        /// returns the the opening hours for the given weekday. Weekday is given as an integer with 0 = sunday, 1 = monday etc.
        /// </summary>
        /// <param name="dayOfWeek"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="isOpen"></param>
        public void GetDay(DayOfWeek dayOfWeek, out DateTime? startTime, out DateTime? endTime, out bool isOpen)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday: startTime = SundayStart; endTime = SundayEnd; isOpen = SundayOpen; break;
                case DayOfWeek.Monday: startTime = MondayStart; endTime = MondayEnd; isOpen = MondayOpen; break;
                case DayOfWeek.Tuesday: startTime = TuesdayStart; endTime = TuesdayEnd; isOpen = TuesdayOpen; break;
                case DayOfWeek.Wednesday: startTime = WednessdayStart; endTime = WednessdayEnd; isOpen = WednessdayOpen; break;
                case DayOfWeek.Thursday: startTime = ThursdayStart; endTime = ThursdayEnd; isOpen = ThursdayOpen; break;
                case DayOfWeek.Friday: startTime = FridayStart; endTime = FridayEnd; isOpen = FridayOpen; break;
                case DayOfWeek.Saturday: startTime = SaturdayStart; endTime = SaturdayEnd; isOpen = SaturdayOpen; break;
                default: startTime = null; endTime = null; isOpen = false; break;
            }

        }

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
