using System.ComponentModel.DataAnnotations;

namespace Trucks.Data.Models
{
    public class Despatcher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40, MinimumLength = 2)]
        public string Name { get; set; } = null!;

        public string? Position { get; set; }

        public ICollection<Truck> Trucks { get; set; } = new HashSet<Truck>();
    }
}
