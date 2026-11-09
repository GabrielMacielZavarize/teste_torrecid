using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.ValueObjects;

namespace InventoryManagement.Domain.Tests.Entities;

public class ProductTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static Product CreateProductWithStock(int quantity = 10)
    {
        var product = new Product("SKU-1", "Produto");
        product.StockIn(quantity, UserId);
        return product;
    }

    [Fact]
    public void StockIn_RecordsCompleteMovement()
    {
        var product = new Product("SKU-1", "Produto");

        var movement = product.StockIn(5, UserId, "Compra");

        Assert.Equal(product.Id, movement.ProductId);
        Assert.Equal(MovementType.StockIn, movement.Type);
        Assert.Equal(5, movement.Quantity);
        Assert.Equal(StockBalance.Zero, movement.BalanceBefore);
        Assert.Equal(new StockBalance(5, 0), movement.BalanceAfter);
        Assert.Equal(UserId, movement.UserId);
        Assert.Equal(new StockBalance(5, 0), product.Stock);
    }

    [Fact]
    public void StockIn_WithoutUser_ThrowsAndKeepsBalance()
    {
        var product = new Product("SKU-1", "Produto");

        Assert.Throws<DomainException>(() => product.StockIn(5, Guid.Empty));
        Assert.Equal(StockBalance.Zero, product.Stock);
    }

    [Fact]
    public void Reserve_CreatesActiveReservationAndKeepsPhysical()
    {
        var product = CreateProductWithStock(10);

        var (reservation, movement) = product.Reserve(4, UserId);

        Assert.Equal(ReservationStatus.Active, reservation.Status);
        Assert.Equal(reservation.Id, movement.ReservationId);
        Assert.Equal(new StockBalance(10, 4), product.Stock);
    }

    [Fact]
    public void CancelReservation_WhenActive_ReturnsQuantityToAvailable()
    {
        var product = CreateProductWithStock(10);
        var (reservation, _) = product.Reserve(4, UserId);

        product.CancelReservation(reservation, UserId);

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(new StockBalance(10, 0), product.Stock);
    }

    [Fact]
    public void CancelReservation_Twice_Throws()
    {
        var product = CreateProductWithStock(10);
        var (reservation, _) = product.Reserve(4, UserId);
        product.CancelReservation(reservation, UserId);

        Assert.Throws<DomainException>(() => product.CancelReservation(reservation, UserId));
    }

    [Fact]
    public void FulfillReservation_WhenActive_DecreasesPhysicalAndReserved()
    {
        var product = CreateProductWithStock(10);
        var (reservation, _) = product.Reserve(4, UserId);

        product.FulfillReservation(reservation, UserId);

        Assert.Equal(ReservationStatus.Fulfilled, reservation.Status);
        Assert.Equal(new StockBalance(6, 0), product.Stock);
    }

    [Fact]
    public void AdjustStock_RecordsSignedDifference()
    {
        var product = CreateProductWithStock(10);

        var movement = product.AdjustStock(7, UserId, "Contagem física");

        Assert.Equal(MovementType.Adjustment, movement.Type);
        Assert.Equal(-3, movement.Quantity);
        Assert.Equal(new StockBalance(7, 0), movement.BalanceAfter);
    }

    [Fact]
    public void AdjustStock_WithoutReason_Throws()
    {
        var product = CreateProductWithStock(10);

        Assert.Throws<DomainException>(() => product.AdjustStock(7, UserId, " "));
    }

    [Fact]
    public void Deactivate_WithStock_Throws()
    {
        var product = CreateProductWithStock(1);

        Assert.Throws<DomainException>(() => product.Deactivate());
    }

    [Fact]
    public void InactiveProduct_RejectsMovements()
    {
        var product = new Product("SKU-1", "Produto");
        product.Deactivate();

        Assert.Throws<DomainException>(() => product.StockIn(1, UserId));
    }
}
