namespace ClinicManagementSystem.api.EntitiesConfigurations
{
    public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
    {
        public void Configure(EntityTypeBuilder<Clinic> builder)
        {
            builder.Property(c => c.Name_En)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Name_Ar)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Address_En)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(c => c.Address_Ar)
                .IsRequired()
                .HasMaxLength(250);
        }
    }
}
