using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Products;

public sealed record CreateProductRequest(string Sku, string Name, string? Description);

public sealed record UpdateProductRequest(string Name, string? Description);

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    bool IsActive,
    int PhysicalQuantity,
    int ReservedQuantity,
    int AvailableQuantity)
{
    public static ProductDto From(Product product) => new(
        product.Id,
        product.Sku,
        product.Name,
        product.Description,
        product.IsActive,
        product.Stock.Physical,
        product.Stock.Reserved,
        product.Stock.Available);
}
