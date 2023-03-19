using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace P03_SalesDatabase.Data.Models
{
    public class Product
    {
        public Product()
        {
            this.Sales = new HashSet<Sale>();
        }

        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        [Unicode(true)]
        public string Name { get; set; } = null!;

        [Required]
        public double Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        [StringLength(250)]
        public string Description { get; set; } = "No description";

        public ICollection<Sale> Sales { get; set; }
    }
}
