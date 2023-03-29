using System.ComponentModel.DataAnnotations;
using VaporStore.Data.Models.Enums;

namespace VaporStore.DataProcessor.ImportDto
{
    public class ImportUserDto
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Username { get; set; } = null!;

        [Required]
        [RegularExpression(@"[A-Z][a-z]+ [A-Z][a-z]+$")]
        public string FullName { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        [Range(3, 103)]
        public int Age { get; set; }

        public UserCardDto[] Cards { get; set; }
    }

    public class UserCardDto
    {
        [Required]
        [RegularExpression(@"\d{4} \d{4} \d{4} \d{4}$")]
        public string Number { get; set; } = null!;

        [Required]
        [RegularExpression(@"\d{3}$")]
        public string Cvc { get; set; } = null!;

        [Required]
        public string Type { get; set; }
    }
}
