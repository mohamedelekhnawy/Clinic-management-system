using ClinicManagementSystem.api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.api.EntitiesConfigurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        builder.Property(p => p.Gender)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Address_En)
            .HasMaxLength(500);

        builder.Property(p => p.Address_Ar)
            .HasMaxLength(500);

        builder.Property(p => p.EmergencyContactName)
            .HasMaxLength(200);

        builder.Property(p => p.EmergencyContactPhone)
            .HasMaxLength(11);

        builder.Property(p => p.Notes)
            .HasMaxLength(2000);

        // Configure ProfileId foreign key
        builder.Property(p => p.ProfileId)
            .IsRequired();

        builder.HasIndex(p => p.ProfileId)
            .IsUnique()
            .HasDatabaseName("IX_Patients_ProfileId");

        builder.HasOne(p => p.Profile)
            .WithOne()
            .HasForeignKey<Patient>(p => p.ProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        // Auditable fields
        builder.Property(p => p.CreatedOn)
            .IsRequired();

        builder.Property(p => p.UpdatedOn);

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(256);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(256);
    }
}
