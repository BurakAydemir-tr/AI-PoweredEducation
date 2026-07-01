using Microsoft.AspNetCore.Identity;
using AI.PoweredEducation.Entity.Entities;

namespace AI.PoweredEducation.Entity.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public ICollection<LearningGame> LearningGames { get; set; } = new List<LearningGame>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
