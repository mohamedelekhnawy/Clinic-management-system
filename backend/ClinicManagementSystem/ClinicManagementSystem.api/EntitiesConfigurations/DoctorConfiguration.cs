
namespace ClinicManagementSystem.api.EntitiesConfigurations
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.Property(c => c.FirstName_En)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.FirstName_Ar)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LastName_En)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LastName_Ar)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Specialty_En)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.Specialty_Ar)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.Description_En)
                .HasMaxLength(1000);

            builder.Property(c => c.Description_Ar)
                .HasMaxLength(1000);

            builder.Property(c => c.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(c => c.SessionPrice)
                .IsRequired()
                .HasPrecision(18, 2);
        }
    }
}
