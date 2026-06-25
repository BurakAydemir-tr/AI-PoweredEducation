using AI.PoweredEducation.Entity.Common;

namespace AI.PoweredEducation.Entity.Identity;

public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public Guid ConcurrencyStamp { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
