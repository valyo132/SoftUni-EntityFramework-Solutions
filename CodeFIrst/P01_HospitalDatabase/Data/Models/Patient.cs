using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace P01_HospitalDatabase.Data.Models
{
    public class Patient
    {
        public Patient()
        {
            this.Prescriptions = new HashSet<PatientMedicament>();
            this.Diagnoses = new HashSet<Diagnose>();
            this.Visitations = new HashSet<Visitation>();
        }

        [Key]
        public int PatientId { get; set; }

        [Required]
        [StringLength(50)]
        [Unicode(true)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        [Unicode(true)]
        public string LastName { get; set; } = null!;

        [Required]
        [StringLength(250)]
        [Unicode(true)]
        public string Address { get; set; } = null!;

        [Required]
        [StringLength(80)]
        [Unicode(false)]
        public string Email { get; set; } = null!;

        [Required]
        public bool HasInsurance { get; set; }

        public ICollection<PatientMedicament> Prescriptions { get; set; }

        public ICollection<Diagnose> Diagnoses { get; set; }

        public ICollection<Visitation> Visitations { get; set; }
    }
}
