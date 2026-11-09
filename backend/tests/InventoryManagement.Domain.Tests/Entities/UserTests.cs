using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_NormalizesUsername()
    {
        var user = new User("  Operador ", "hash", Role.Operator);

        Assert.Equal("operador", user.Username);
        Assert.Equal(Role.Operator, user.Role);
    }

    [Theory]
    [InlineData("", "hash")]
    [InlineData("   ", "hash")]
    [InlineData("operador", "")]
    public void Constructor_WithoutRequiredFields_Throws(string username, string passwordHash)
    {
        Assert.Throws<DomainException>(() => new User(username, passwordHash, Role.Operator));
    }

    [Fact]
    public void Constructor_WithUsernameTooLong_Throws()
    {
        var username = new string('a', User.UsernameMaxLength + 1);

        Assert.Throws<DomainException>(() => new User(username, "hash", Role.Operator));
    }

    [Fact]
    public void Constructor_WithInvalidRole_Throws()
    {
        Assert.Throws<DomainException>(() => new User("operador", "hash", (Role)99));
    }
}
