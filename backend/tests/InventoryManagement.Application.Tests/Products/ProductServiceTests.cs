using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Exceptions;
using InventoryManagement.Application.Products;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using NSubstitute;

namespace InventoryManagement.Application.Tests.Products;

public class ProductServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_products, _unitOfWork);
    }

    [Fact]
    public async Task CreateAsync_WithNewSku_AddsProductAndSaves()
    {
        var result = await _service.CreateAsync(new CreateProductRequest("sku-1", "Produto", null));

        Assert.Equal("SKU-1", result.Sku);
        _products.Received(1).Add(Arg.Is<Product>(product => product.Sku == "SKU-1"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSku_ThrowsConflictAndDoesNotSave()
    {
        _products.SkuExistsAsync("SKU-1", Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(new CreateProductRequest("sku-1", "Produto", null)));

        _products.DidNotReceive().Add(Arg.Any<Product>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithExistingProduct_ChangesDetailsAndSaves()
    {
        var product = StubProduct(new Product("SKU-1", "Produto"));

        var result = await _service.UpdateAsync(product.Id, new UpdateProductRequest("Novo nome", "Descrição"));

        Assert.Equal("Novo nome", result.Name);
        Assert.Equal("Descrição", result.Description);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownProduct_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _products.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.UpdateAsync(id, new UpdateProductRequest("Novo nome", null)));
    }

    [Fact]
    public async Task DeleteAsync_WithZeroStock_DeactivatesAndSaves()
    {
        var product = StubProduct(new Product("SKU-1", "Produto"));

        await _service.DeleteAsync(product.Id);

        Assert.False(product.IsActive);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WithStock_ThrowsAndDoesNotSave()
    {
        var product = new Product("SKU-1", "Produto");
        product.StockIn(1, UserId);
        StubProduct(product);

        await Assert.ThrowsAsync<DomainException>(() => _service.DeleteAsync(product.Id));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private Product StubProduct(Product product)
    {
        _products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        return product;
    }
}
