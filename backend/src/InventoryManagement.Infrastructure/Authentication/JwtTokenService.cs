using System.Security.Claims;
using System.Text;
using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Auth;
using InventoryManagement.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Infrastructure.Authentication;

internal sealed class JwtTokenService(JwtOptions options) : ITokenService
{
    public AccessToken CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(options.ExpirationMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Issuer,
            Audience = options.Audience,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(AppClaimTypes.UserId, user.Id.ToString()),
                new Claim(AppClaimTypes.Username, user.Username),
                new Claim(AppClaimTypes.Role, user.Role.ToString())
            ])
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new AccessToken(token, expiresAt);
    }
}
