using InventoryManagement.Application.Auth;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Abstractions;

public interface ITokenService
{
    AccessToken CreateToken(User user);
}
