using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace VaporStore.Data.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

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

        public ICollection<Card> Cards { get; set; } = new HashSet<Card>(); 
    }
}
