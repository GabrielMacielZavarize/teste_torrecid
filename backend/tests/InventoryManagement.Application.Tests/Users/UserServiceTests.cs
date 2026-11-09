using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Exceptions;
using InventoryManagement.Application.Users;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using NSubstitute;

namespace InventoryManagement.Application.Tests.Users;

public class UserServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(_users, _passwordHasher, _unitOfWork);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_StoresHashNotPlainPassword()
    {
        _passwordHasher.Hash("senha-segura").Returns("hash-gerado");

        var result = await _service.CreateAsync(new CreateUserRequest("Novo", "senha-segura", Role.Operator));

        Assert.Equal("novo", result.Username);
        _users.Received(1).Add(Arg.Is<User>(user => user.PasswordHash == "hash-gerado"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithExistingUsername_ThrowsConflictAndDoesNotSave()
    {
        _users.UsernameExistsAsync("novo", Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(new CreateUserRequest("novo", "senha-segura", Role.Operator)));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(7)]
    [InlineData(73)]
    public async Task CreateAsync_WithInvalidPasswordLength_ThrowsAndDoesNotHash(int length)
    {
        var request = new CreateUserRequest("novo", new string('a', length), Role.Operator);

        await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(request));

        _passwordHasher.DidNotReceive().Hash(Arg.Any<string>());
    }
}
