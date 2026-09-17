namespace ClinicManagementSystem.api.EntitiesConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Configure ProfileId foreign key
            builder.Property(u => u.ProfileId)
                .IsRequired(false);

            builder.HasIndex(u => u.ProfileId)
                .IsUnique()
                .HasFilter("[ProfileId] IS NOT NULL");

            builder.HasOne(u => u.Profile)
                .WithOne(p => p.ApplicationUser)
                .HasForeignKey<ApplicationUser>(u => u.ProfileId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

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

            // Prevent automatic loading of RefreshTokens collection
            builder.Navigation(u => u.RefreshTokens)
                .AutoInclude(false);
        }
    }
}
