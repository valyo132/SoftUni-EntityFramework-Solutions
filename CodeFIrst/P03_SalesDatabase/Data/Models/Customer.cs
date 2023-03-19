using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace P03_SalesDatabase.Data.Models
{
    public class Customer
    {
        public Customer()
        {
            this.Sales = new HashSet<Sale>();
        }

        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        [Unicode(true)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(80)]
        [Unicode(false)]
        public string Email { get; set; } = null!;

        [Required]
        public string CreditCardNumber { get; set; } = null!;

        public ICollection<Sale> Sales { get; set; }
    }
}
