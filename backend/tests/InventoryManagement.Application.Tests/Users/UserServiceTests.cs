using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Users;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using NSubstitute;

namespace InventoryManagement.Application.Tests.Users;

public class UserServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateAsync_StoresHashNotPlainPassword()
    {
        _passwordHasher.Hash("senha-segura").Returns("hash-gerado");
        var service = new UserService(_users, _passwordHasher, _unitOfWork);

        await service.CreateAsync(new CreateUserRequest("novo", "senha-segura", Role.Operator));

        _users.Received(1).Add(Arg.Is<User>(user => user.PasswordHash == "hash-gerado"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
