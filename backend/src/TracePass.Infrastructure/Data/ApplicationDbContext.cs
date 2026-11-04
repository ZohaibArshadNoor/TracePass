using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TracePass.Domain.Entities;
using TracePass.Infrastructure.Identity;

namespace TracePass.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Material> Materials => Set<Material>();

    public DbSet<SupplierProfile> SupplierProfiles => Set<SupplierProfile>();

    public DbSet<SupplierMaterialCategory> SupplierMaterialCategories => Set<SupplierMaterialCategory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Organization>(entity =>
        {
            entity.HasKey(organization => organization.Id);

            entity.Property(organization => organization.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(organization => organization.Country)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(organization => organization.RegistrationNumber)
                .HasMaxLength(100);

            entity.Property(organization => organization.ContactEmail)
                .HasMaxLength(256);

            entity.Property(organization => organization.ContactPhone)
                .HasMaxLength(30);

            entity.Property(organization => organization.Type)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasMany<ApplicationUser>()
                .WithOne(user => user.Organization)
                .HasForeignKey(user => user.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.LastName)
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(token => token.Id);

            entity.Property(token => token.TokenHash)
                .HasMaxLength(128)
                .IsRequired();

            entity.HasIndex(token => token.TokenHash)
                .IsUnique();

            entity.HasIndex(token => token.UserId);

            entity.Property(token => token.RevokedAt)
                .IsConcurrencyToken();

            entity.HasOne(token => token.User)
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Material>(entity =>
        {
            entity.HasKey(material => material.Id);

            entity.Property(material => material.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(material => material.Category)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(material => material.OriginCountry)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(material => material.Notes)
                .HasMaxLength(1000);

            entity.HasIndex(material => material.Name);
            entity.HasIndex(material => material.Category);
        });

        builder.Entity<SupplierProfile>(entity =>
        {
            entity.HasKey(profile => profile.Id);

            entity.Property(profile => profile.Rating)
                .HasPrecision(3, 2);

            entity.Property(profile => profile.Notes)
                .HasMaxLength(1000);

            entity.HasOne(profile => profile.Organization)
                .WithOne()
                .HasForeignKey<SupplierProfile>(profile => profile.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SupplierMaterialCategory>(entity =>
        {
            entity.HasKey(smc => new { smc.SupplierProfileId, smc.Category });

            entity.Property(smc => smc.Category)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasOne(smc => smc.SupplierProfile)
                .WithMany(profile => profile.MaterialCategories)
                .HasForeignKey(smc => smc.SupplierProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
