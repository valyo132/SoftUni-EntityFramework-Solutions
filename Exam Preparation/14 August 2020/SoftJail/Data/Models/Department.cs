using System.ComponentModel.DataAnnotations;

namespace SoftJail.Data.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 3)]
        public string Name { get; set; } = null!;

        public ICollection<Cell> Cells { get; set; } = new HashSet<Cell>();
    }
}
