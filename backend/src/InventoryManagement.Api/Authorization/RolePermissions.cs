using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Api.Authorization;

// Único lugar que define o que cada papel pode fazer.
public static class RolePermissions
{
    private static readonly Dictionary<Role, string[]> PermissionsByRole = new()
    {
        [Role.Administrator] = Permissions.All,
        [Role.Operator] = [Permissions.ProductsView, Permissions.StockMove, Permissions.StockViewHistory],
        [Role.Consultant] = [Permissions.ProductsView, Permissions.StockViewHistory]
    };

    public static IReadOnlyCollection<string> For(Role role)
    {
        return PermissionsByRole[role];
    }

    public static string[] RolesWith(string permission)
    {
        return PermissionsByRole
            .Where(entry => entry.Value.Contains(permission))
            .Select(entry => entry.Key.ToString())
            .ToArray();
    }
}
