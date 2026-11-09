namespace InventoryManagement.Api.Authorization;

public static class Permissions
{
    public const string ProductsView = "Products.View";
    public const string ProductsManage = "Products.Manage";
    public const string StockMove = "Stock.Move";
    public const string StockViewHistory = "Stock.ViewHistory";
    public const string UsersManage = "Users.Manage";

    public static readonly string[] All =
    [
        ProductsView,
        ProductsManage,
        StockMove,
        StockViewHistory,
        UsersManage
    ];
}
