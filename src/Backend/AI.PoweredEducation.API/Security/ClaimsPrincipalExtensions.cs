using System.Security.Claims;

namespace AI.PoweredEducation.API.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("Authenticated user identifier is missing.");
        }

        return userId;
    }
}
