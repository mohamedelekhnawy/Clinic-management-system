using ClinicManagementSystem.api.Models;
using ClinicManagementSystem.api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.api.EntitiesConfigurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.AppointmentDate)
            .IsRequired();

        builder.Property(a => a.StartTime)
            .IsRequired();

        builder.Property(a => a.EndTime)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(AppointmentStatus.Scheduled);

        builder.Property(a => a.CheckedInAt)
            .IsRequired(false);

        builder.Property(a => a.ActualStartTime)
            .IsRequired(false);

        builder.Property(a => a.ActualEndTime)
            .IsRequired(false);

        builder.Property(a => a.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);

        // ========== Relationships ==========
        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.StartTime })
            .HasDatabaseName("IX_Appointments_Doctor_Date_Time");

        builder.HasIndex(a => new { a.PatientId, a.AppointmentDate })
            .HasDatabaseName("IX_Appointments_Patient_Date");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_Appointments_Status");

        builder.HasIndex(a => new { a.Status, a.AppointmentDate })
            .HasDatabaseName("IX_Appointments_Status_Date");
    }
}
