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

        // Foreign key to Clinic
        builder.HasOne(a => a.Clinic)
            .WithMany()
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ProfileId foreign key
        builder.Property(a => a.ProfileId)
            .IsRequired();

        builder.HasIndex(a => a.ProfileId)
            .IsUnique()
            .HasDatabaseName("IX_Assistants_ProfileId");

        builder.HasOne(a => a.Profile)
            .WithOne()
            .HasForeignKey<Assistant>(a => a.ProfileId)
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
