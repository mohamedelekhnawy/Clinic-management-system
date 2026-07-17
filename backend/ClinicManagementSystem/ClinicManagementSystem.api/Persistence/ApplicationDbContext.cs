using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ClinicManagementSystem.api.Persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):IdentityDbContext(options)
    {
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Doctor> Doctor { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
