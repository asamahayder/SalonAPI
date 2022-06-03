using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models
{
    [Index(nameof(Key), IsUnique = true)]
    public class Text
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public string DanishValue { get; set; }

        [Required]
        public string EnglishValue { get; set; }
    }
}
