using MediaTR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaTR.Infrastructure.Configuration;

/// <summary>
/// EF Core entity configuration for RefreshToken.
/// Defines database schema, indexes, and constraints for JWT refresh tokens.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("RefreshTokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.JwtId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.Property(e => e.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.RevokedAt)
            .IsRequired(false);

        builder.Property(e => e.CreatedByIp)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(e => e.RevokedByIp)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Indexes for query performance
        builder.HasIndex(e => e.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        builder.HasIndex(e => new { e.UserId, e.IsRevoked, e.ExpiresAt })
            .HasDatabaseName("IX_RefreshTokens_UserId_IsRevoked_ExpiresAt");

        builder.HasIndex(e => e.JwtId)
            .HasDatabaseName("IX_RefreshTokens_JwtId");

        // Filtered index for non-revoked tokens only (SQL Server specific)
        // Note: Cannot use GETUTCDATE() in filter - non-deterministic functions not allowed
        builder.HasIndex(e => new { e.UserId, e.ExpiresAt })
            .HasDatabaseName("IX_RefreshTokens_Active")
            .HasFilter("[IsRevoked] = 0");
    }
}
