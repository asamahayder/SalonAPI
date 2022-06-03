using System.ComponentModel.DataAnnotations;

namespace SalonAPI.Models.DTOs
{
    public class TextDTO
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
