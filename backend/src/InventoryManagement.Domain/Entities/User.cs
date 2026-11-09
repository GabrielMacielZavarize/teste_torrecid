using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Domain.Entities;

public class User
{
    public const int UsernameMaxLength = 50;
    public const int PasswordHashMaxLength = 100;

    public Guid Id { get; private init; }
    public string Username { get; private init; }
    public string PasswordHash { get; private init; }
    public Role Role { get; private init; }

    private User()
    {
        Username = null!;
        PasswordHash = null!;
    }

    public User(string username, string passwordHash, Role role)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("O hash da senha é obrigatório.");
        if (!Enum.IsDefined(role))
            throw new DomainException("Papel de usuário inválido.");

        Id = Guid.NewGuid();
        Username = NormalizeUsername(username);
        PasswordHash = passwordHash;
        Role = role;
    }

    public static string NormalizeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("O nome de usuário é obrigatório.");

        var normalized = username.Trim().ToLowerInvariant();
        if (normalized.Length > UsernameMaxLength)
            throw new DomainException($"O nome de usuário aceita no máximo {UsernameMaxLength} caracteres.");

        return normalized;
    }
}
