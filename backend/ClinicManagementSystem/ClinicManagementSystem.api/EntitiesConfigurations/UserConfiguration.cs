namespace ClinicManagementSystem.api.EntitiesConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.OwnsMany(u => u.RefreshTokens, rt =>
            {
                rt.ToTable("RefreshTokens");

                rt.WithOwner()
                    .HasForeignKey("UserId");

                rt.Property<string>("UserId")
                    .IsRequired();

                rt.HasKey(nameof(RefreshToken.Id), "UserId");

                rt.Property(t => t.Token)
                    .IsRequired()
                    .HasMaxLength(500);

                rt.Property(t => t.ExpiresOn)
                    .IsRequired();

                rt.Property(t => t.CreatedOn)
                    .IsRequired();

                rt.Property(t => t.RevokedOn)
                    .IsRequired(false);

                rt.HasIndex(t => t.Token)
                    .IsUnique();
            });
        }
    }
}
