using InventoryManagement.Application.Abstractions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Infrastructure.Persistence;

// Executado somente em ambiente de desenvolvimento. As senhas abaixo não valem para produção.
public static class DatabaseInitializer
{
    private static readonly (string Username, string Password, Role Role)[] SeedUsers =
    [
        ("admin", "Admin@12345", Role.Administrator),
        ("operador", "Operador@12345", Role.Operator),
        ("consultor", "Consultor@12345", Role.Consultant)
    ];

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync(cancellationToken);

        if (await context.Users.AnyAsync(cancellationToken))
            return;

        foreach (var (username, password, role) in SeedUsers)
            context.Users.Add(new User(username, passwordHasher.Hash(password), role));

        await context.SaveChangesAsync(cancellationToken);
    }
}
