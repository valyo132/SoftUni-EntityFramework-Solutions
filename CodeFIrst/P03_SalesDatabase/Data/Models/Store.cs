using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace P03_SalesDatabase.Data.Models
{
    public class Store
    {
        public Store()
        {
            this.Sales = new HashSet<Sale>();
        }

        [Key]
        public int StoreId { get; set; }

        [Required]
        [StringLength(80)]
        [Unicode(true)]
        public string Name { get; set; } = null!;

        public ICollection<Sale> Sales { get; set; }
    }
}
