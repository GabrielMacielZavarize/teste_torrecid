using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Exceptions;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Products;

public sealed class ProductService(IProductRepository products, IUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<ProductDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await products.ListActiveAsync(cancellationToken);
        return items.Select(ProductDto.From).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetProductAsync(id, cancellationToken);
        return ProductDto.From(product);
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = new Product(request.Sku, request.Name, request.Description);

        if (await products.SkuExistsAsync(product.Sku, cancellationToken))
            throw new ConflictException($"Já existe um produto com o SKU {product.Sku}.");

        products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductDto.From(product);
    }

    public async Task<ProductDto> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductAsync(id, cancellationToken);

        product.UpdateDetails(request.Name, request.Description);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductDto.From(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetProductAsync(id, cancellationToken);

        product.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Product> GetProductAsync(Guid id, CancellationToken cancellationToken)
    {
        return await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Produto não encontrado.");
    }
}
