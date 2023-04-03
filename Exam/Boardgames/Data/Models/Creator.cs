using System.ComponentModel.DataAnnotations;

namespace Boardgames.Data.Models
{
    public class Creator
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(7, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(7, MinimumLength = 2)]
        public string LastName { get; set; } = null!;

        public ICollection<Boardgame> Boardgames { get; set; } = new HashSet<Boardgame>();
    }
}
