using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Auth;

namespace InventoryManagement.Api.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId => Guid.Parse(GetRequiredClaim(AppClaimTypes.UserId));

    public string Username => GetRequiredClaim(AppClaimTypes.Username);

    private string GetRequiredClaim(string claimType)
    {
        return httpContextAccessor.HttpContext?.User.FindFirst(claimType)?.Value
            ?? throw new InvalidOperationException($"A claim '{claimType}' não está presente no token.");
    }
}
