using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Exceptions;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
{
    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var username = User.NormalizeUsername(request.Username);
        var user = await users.GetByUsernameAsync(username, cancellationToken);

        // Mesmo erro nos dois casos, para não revelar quais usuários existem.
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        return new AuthResult(tokenService.CreateToken(user), user.Username, user.Role);
    }
}
