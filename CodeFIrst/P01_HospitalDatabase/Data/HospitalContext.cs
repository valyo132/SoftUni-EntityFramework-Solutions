using Microsoft.EntityFrameworkCore;
using P01_HospitalDatabase.Data.Models;

namespace P01_HospitalDatabase.Data
{
    public class HospitalContext : DbContext
    {
        public HospitalContext()
        {

        }

        public HospitalContext(DbContextOptions options)
            : base(options)
        {

        }

        // TODO: DbSets<> here
        public DbSet<Diagnose> Diagnoses { get; set; } = null!;

        public DbSet<Medicament> Medicaments { get; set; } = null!;

        public DbSet<Patient> Patients { get; set; } = null!;

        public DbSet<PatientMedicament> PatientMedicaments { get; set; } = null!;

        public DbSet<Visitation> Visitations { get; set; } = null!;

        public DbSet<Doctor> Doctors { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Config.connectionString);
            }
        }

        // TODO: Fluent API here
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PatientMedicament>()
                .HasKey(pm => new { pm.PatientId, pm.MedicamentId });
        }
    }
}
