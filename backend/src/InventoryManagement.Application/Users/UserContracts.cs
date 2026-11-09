using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.Users;

public sealed record CreateUserRequest(string Username, string Password, Role Role);

public sealed record UserDto(Guid Id, string Username, Role Role)
{
    public static UserDto From(User user) => new(user.Id, user.Username, user.Role);
}
