using AI.PoweredEducation.Entity.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.PoweredEducation.DataAccess.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.Id);

        builder.Property(token => token.CreatedAt).IsRequired();
        builder.Property(token => token.TokenHash)
            .IsRequired()
            .HasMaxLength(64);
        builder.Property(token => token.ReplacedByTokenHash)
            .HasMaxLength(64);
        builder.Property(token => token.ExpiresAt).IsRequired();
        builder.Property(token => token.ConcurrencyStamp)
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasIndex(token => token.TokenHash)
            .IsUnique();

        builder.HasIndex(token => new { token.UserId, token.ExpiresAt });

        builder.HasOne(token => token.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
