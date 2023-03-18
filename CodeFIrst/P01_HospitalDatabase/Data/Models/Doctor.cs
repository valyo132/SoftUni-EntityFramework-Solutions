using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace P01_HospitalDatabase.Data.Models
{
    public class Doctor
    {
        public Doctor()
        {
            this.Visitations = new HashSet<Visitation>();
        }

        [Key]
        public int DoctorId { get; set; }

        [Required]
        [StringLength(100)]
        [Unicode(true)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [Unicode(true)]
        public string Specialty { get; set; } = null!;

        public ICollection<Visitation> Visitations { get; set; }
    }
}
