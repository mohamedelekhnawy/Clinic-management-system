namespace ClinicManagementSystem.api.EntitiesConfigurations
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.Property(d => d.Specialty_En)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(d => d.Specialty_Ar)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(d => d.Description_En)
                .HasMaxLength(1000);

            builder.Property(d => d.Description_Ar)
                .HasMaxLength(1000);

            builder.Property(d => d.SessionPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            // Configure ProfileId foreign key
            builder.Property(d => d.ProfileId)
                .IsRequired();

            builder.HasIndex(d => d.ProfileId)
                .IsUnique()
                .HasDatabaseName("IX_Doctor_ProfileId");

            builder.HasOne(d => d.Profile)
                .WithOne()
                .HasForeignKey<Doctor>(d => d.ProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
