using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Auth;
using InventoryManagement.Application.Exceptions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using NSubstitute;

namespace InventoryManagement.Application.Tests.Auth;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_users, _passwordHasher, _tokenService);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndNormalizesUsername()
    {
        var user = StubUser();
        _passwordHasher.Verify("senha-correta", user.PasswordHash).Returns(true);
        var token = new AccessToken("token", DateTime.UtcNow.AddHours(2));
        _tokenService.CreateToken(user).Returns(token);

        var result = await _service.LoginAsync(new LoginRequest("  ADMIN ", "senha-correta"));

        Assert.Equal(token, result.AccessToken);
        Assert.Equal("admin", result.Username);
        Assert.Equal(Role.Administrator, result.Role);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsAndCreatesNoToken()
    {
        var user = StubUser();
        _passwordHasher.Verify("senha-errada", user.PasswordHash).Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _service.LoginAsync(new LoginRequest("admin", "senha-errada")));

        _tokenService.DidNotReceive().CreateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUser_ThrowsSameException()
    {
        _users.GetByUsernameAsync("ninguem", Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _service.LoginAsync(new LoginRequest("ninguem", "qualquer-senha")));
    }

    private User StubUser()
    {
        var user = new User("admin", "hash", Role.Administrator);
        _users.GetByUsernameAsync("admin", Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }
}
