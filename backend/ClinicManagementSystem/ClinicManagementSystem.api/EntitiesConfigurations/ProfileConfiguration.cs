using ClinicManagementSystem.api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.api.EntitiesConfigurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");

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

        builder.Property(p => p.Phone)
            .IsRequired()
            .HasMaxLength(20);

        // Create unique index on Phone
        builder.HasIndex(p => p.Phone)
            .IsUnique()
            .HasDatabaseName("IX_Profiles_Phone");

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(255);

        // Create unique index on Email
        builder.HasIndex(p => p.Email)
            .IsUnique()
            .HasDatabaseName("IX_Profiles_Email");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Foreign key to ApplicationUser (nullable)
        builder.HasOne(p => p.ApplicationUser)
            .WithOne(u => u.Profile)
            .HasForeignKey<Profile>(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

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
