using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Exceptions;
using InventoryManagement.Application.Stock;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using NSubstitute;

namespace InventoryManagement.Application.Tests.Stock;

public class StockServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IReservationRepository _reservations = Substitute.For<IReservationRepository>();
    private readonly IStockMovementRepository _movements = Substitute.For<IStockMovementRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly StockService _service;

    public StockServiceTests()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(UserId);
        currentUser.Username.Returns("operador");

        _service = new StockService(_products, _reservations, _movements, _unitOfWork, currentUser);
    }

    [Fact]
    public async Task StockInAsync_RegistersMovementWithCurrentUserAndSaves()
    {
        var product = StubProductWithStock(10);

        var result = await _service.StockInAsync(product.Id, new StockOperationRequest(5, "Compra"));

        Assert.Equal(15, result.PhysicalAfter);
        Assert.Equal("operador", result.Username);
        _movements.Received(1).Add(Arg.Is<StockMovement>(movement => movement.UserId == UserId));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StockInAsync_WithUnknownProduct_ThrowsNotFoundAndDoesNotSave()
    {
        var id = Guid.NewGuid();
        _products.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.StockInAsync(id, new StockOperationRequest(5, null)));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StockOutAsync_AboveAvailable_ThrowsAndRegistersNothing()
    {
        var product = StubProductWithStock(10);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.StockOutAsync(product.Id, new StockOperationRequest(11, null)));

        _movements.DidNotReceive().Add(Arg.Any<StockMovement>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReserveAsync_AddsReservationAndMovementAndSaves()
    {
        var product = StubProductWithStock(10);

        var result = await _service.ReserveAsync(product.Id, new StockOperationRequest(4, null));

        Assert.Equal(ReservationStatus.Active, result.Status);
        _reservations.Received(1).Add(Arg.Is<Reservation>(reservation => reservation.Quantity == 4));
        _movements.Received(1).Add(Arg.Is<StockMovement>(movement => movement.Type == MovementType.Reservation));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private Product StubProductWithStock(int quantity)
    {
        var product = new Product("SKU-1", "Produto");
        product.StockIn(quantity, UserId);
        _products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        return product;
    }
}
