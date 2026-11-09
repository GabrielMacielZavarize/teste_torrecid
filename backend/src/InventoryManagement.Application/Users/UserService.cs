using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Exceptions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Application.Users;

public sealed class UserService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 72; 

    public async Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await users.ListAsync(cancellationToken);
        return items.Select(UserDto.From).ToList();
    }

    public async Task<UserDto> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Password.Length is < MinPasswordLength or > MaxPasswordLength)
            throw new DomainException($"A senha deve ter entre {MinPasswordLength} e {MaxPasswordLength} caracteres.");

        var username = User.NormalizeUsername(request.Username);

        if (await users.UsernameExistsAsync(username, cancellationToken))
            throw new ConflictException($"Já existe um usuário com o nome {username}.");

        var user = new User(username, passwordHasher.Hash(request.Password), request.Role);

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
