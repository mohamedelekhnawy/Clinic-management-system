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

        builder.Property(p => p.FirstName_En)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.FirstName_Ar)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastName_En)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastName_Ar)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        builder.Property(p => p.Gender)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Phone)
            .IsRequired()
            .HasMaxLength(11);

        // Create unique index on Phone
        builder.HasIndex(p => p.Phone)
            .IsUnique()
            .HasDatabaseName("IX_Patients_Phone");

        builder.Property(p => p.Email)
            .HasMaxLength(200);

        // Create unique index on Email (filtered for non-null values)
        builder.HasIndex(p => p.Email)
            .IsUnique()
            .HasDatabaseName("IX_Patients_Email")
            .HasFilter("[Email] IS NOT NULL");

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

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

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
