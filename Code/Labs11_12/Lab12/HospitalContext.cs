using Microsoft.EntityFrameworkCore;

namespace Lab12;

public class HospitalContext(DbContextOptions<HospitalContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Admission> Admissions => Set<Admission>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Admission>().HasOne(a => a.Patient).WithMany()
            .HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Admission>().HasOne(a => a.Doctor).WithMany()
            .HasForeignKey(a => a.DoctorId).OnDelete(DeleteBehavior.Restrict);
    }
}
