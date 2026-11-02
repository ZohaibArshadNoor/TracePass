
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
    }



}