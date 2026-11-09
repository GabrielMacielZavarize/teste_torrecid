using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Exceptions;
using InventoryManagement.Application.Products;
using InventoryManagement.Domain.Entities;
using NSubstitute;

namespace InventoryManagement.Application.Tests.Products;

public class ProductServiceTests
{
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_products, _unitOfWork);
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
}
