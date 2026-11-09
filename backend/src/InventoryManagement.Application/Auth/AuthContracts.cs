using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.Auth;

public static class AppClaimTypes
{
    public const string UserId = "sub";
    public const string Username = "name";
    public const string Role = "role";
}

public sealed record LoginRequest(string Username, string Password);

public sealed record AccessToken(string Value, DateTime ExpiresAt);

public sealed record AuthResult(AccessToken AccessToken, string Username, Role Role);
