using ClinicManagementSystem.api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.api.EntitiesConfigurations;

public class AssistantConfiguration : IEntityTypeConfiguration<Assistant>
{
    public void Configure(EntityTypeBuilder<Assistant> builder)
    {
        builder.ToTable("Assistants");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.FirstName_En)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.FirstName_Ar)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LastName_En)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LastName_Ar)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Phone)
            .IsRequired()
            .HasMaxLength(11);

        // Create unique index on Phone
        builder.HasIndex(a => a.Phone)
            .IsUnique()
            .HasDatabaseName("IX_Assistants_Phone");

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(200);

        // Create unique index on Email
        builder.HasIndex(a => a.Email)
            .IsUnique()
            .HasDatabaseName("IX_Assistants_Email");

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Foreign key to Clinic
        builder.HasOne(a => a.Clinic)
            .WithMany()
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        // Auditable fields
        builder.Property(a => a.CreatedOn)
            .IsRequired();

        builder.Property(a => a.UpdatedOn);

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(256);

        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(256);
    }
}
