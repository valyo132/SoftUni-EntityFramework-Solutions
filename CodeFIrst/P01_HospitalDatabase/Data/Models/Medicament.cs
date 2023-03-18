using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace P01_HospitalDatabase.Data.Models
{
    public class Medicament
    {
        public Medicament()
        {
            this.Prescriptions = new HashSet<PatientMedicament>();
        }

        [Key]
        public int MedicamentId { get; set; }

        [Required]
        [StringLength(50)]
        [Unicode(true)]
        public string Name { get; set; } = null!;

        public ICollection<PatientMedicament> Prescriptions { get; set; }
    }
}
