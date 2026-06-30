using System.Security.Claims;

namespace BugBoard26.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    private static readonly string[] UserIdClaimTypes =
    [
        ClaimTypes.NameIdentifier,
        "sub",
        "userId"
    ];

    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        foreach (var claimType in UserIdClaimTypes)
        {
            var value = principal.FindFirstValue(claimType);

            if (int.TryParse(value, out var userId))
            {
                return userId;
            }
        }

        return null;
    }
}
