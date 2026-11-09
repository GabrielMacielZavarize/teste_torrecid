using InventoryManagement.Api.Authorization;
using InventoryManagement.Application.Auth;
using InventoryManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controllers;

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    string Username,
    Role Role,
    IReadOnlyCollection<string> Permissions);

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<LoginResponse> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        return new LoginResponse(
            result.AccessToken.Value,
            result.AccessToken.ExpiresAt,
            result.Username,
            result.Role,
            RolePermissions.For(result.Role));
    }
}
