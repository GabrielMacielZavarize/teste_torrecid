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
    public void Constructor_WithValidData_CreatesActiveProductWithZeroStock()
    {
        var product = new Product("  sku-1 ", " Produto ");

        Assert.Equal("SKU-1", product.Sku);
        Assert.Equal("Produto", product.Name);
        Assert.True(product.IsActive);
        Assert.Equal(StockBalance.Zero, product.Stock);
    }

    [Theory]
    [InlineData("", "Produto")]
    [InlineData("   ", "Produto")]
    [InlineData("SKU-1", "")]
    public void Constructor_WithoutRequiredFields_Throws(string sku, string name)
    {
        Assert.Throws<DomainException>(() => new Product(sku, name));
    }

    [Fact]
    public void Constructor_WithNameTooLong_Throws()
    {
        var name = new string('a', Product.NameMaxLength + 1);

        Assert.Throws<DomainException>(() => new Product("SKU-1", name));
    }

    [Fact]
    public void UpdateDetails_OnActiveProduct_ChangesValues()
    {
        var product = new Product("SKU-1", "Produto");

        product.UpdateDetails("Novo nome", "Nova descrição");

        Assert.Equal("Novo nome", product.Name);
        Assert.Equal("Nova descrição", product.Description);
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
        Assert.Equal("Compra", movement.Reason);
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
    public void StockOut_ConsideringReservations_ThrowsWhenAboveAvailable()
    {
        var product = CreateProductWithStock(10);
        product.Reserve(8, UserId);

        Assert.Throws<DomainException>(() => product.StockOut(3, UserId));
    }

    [Fact]
    public void Reserve_CreatesActiveReservationAndKeepsPhysical()
    {
        var product = CreateProductWithStock(10);

        var (reservation, movement) = product.Reserve(4, UserId);

        Assert.Equal(ReservationStatus.Active, reservation.Status);
        Assert.Equal(product.Id, reservation.ProductId);
        Assert.Equal(reservation.Id, movement.ReservationId);
        Assert.Equal(MovementType.Reservation, movement.Type);
        Assert.Equal(new StockBalance(10, 4), product.Stock);
    }

    [Fact]
    public void CancelReservation_WhenActive_ReturnsQuantityToAvailable()
    {
        var product = CreateProductWithStock(10);
        var (reservation, _) = product.Reserve(4, UserId);

        var movement = product.CancelReservation(reservation, UserId);

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(MovementType.ReservationCancellation, movement.Type);
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
    public void CancelReservation_FromAnotherProduct_Throws()
    {
        var product = CreateProductWithStock(10);
        var anotherProduct = CreateProductWithStock(10);
        var (reservation, _) = anotherProduct.Reserve(4, UserId);

        Assert.Throws<DomainException>(() => product.CancelReservation(reservation, UserId));
    }

    [Fact]
    public void FulfillReservation_WhenActive_DecreasesPhysicalAndReserved()
    {
        var product = CreateProductWithStock(10);
        var (reservation, _) = product.Reserve(4, UserId);

        var movement = product.FulfillReservation(reservation, UserId);

        Assert.Equal(ReservationStatus.Fulfilled, reservation.Status);
        Assert.Equal(MovementType.ReservationFulfillment, movement.Type);
        Assert.Equal(new StockBalance(6, 0), product.Stock);
    }

    [Fact]
    public void FulfillReservation_AfterCancellation_Throws()
    {
        var product = CreateProductWithStock(10);
        var (reservation, _) = product.Reserve(4, UserId);
        product.CancelReservation(reservation, UserId);

        Assert.Throws<DomainException>(() => product.FulfillReservation(reservation, UserId));
    }

    [Fact]
    public void AdjustStock_RecordsSignedDifferenceAndReason()
    {
        var product = CreateProductWithStock(10);

        var movement = product.AdjustStock(7, UserId, "  Contagem física  ");

        Assert.Equal(MovementType.Adjustment, movement.Type);
        Assert.Equal(-3, movement.Quantity);
        Assert.Equal("Contagem física", movement.Reason);
        Assert.Equal(new StockBalance(10, 0), movement.BalanceBefore);
        Assert.Equal(new StockBalance(7, 0), movement.BalanceAfter);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AdjustStock_WithoutReason_Throws(string reason)
    {
        var product = CreateProductWithStock(10);

        Assert.Throws<DomainException>(() => product.AdjustStock(7, UserId, reason));
    }

    [Fact]
    public void AdjustStock_BelowReserved_Throws()
    {
        var product = CreateProductWithStock(10);
        product.Reserve(4, UserId);

        Assert.Throws<DomainException>(() => product.AdjustStock(3, UserId, "Contagem"));
    }

    [Fact]
    public void Deactivate_WithZeroStock_MakesProductInactive()
    {
        var product = new Product("SKU-1", "Produto");

        product.Deactivate();

        Assert.False(product.IsActive);
    }

    [Fact]
    public void Deactivate_WithStock_Throws()
    {
        var product = CreateProductWithStock(1);

        Assert.Throws<DomainException>(() => product.Deactivate());
    }

    [Fact]
    public void StockIn_WithReasonTooLong_ThrowsAndKeepsBalance()
    {
        var product = new Product("SKU-1", "Produto");
        var reason = new string('a', StockMovement.ReasonMaxLength + 1);

        Assert.Throws<DomainException>(() => product.StockIn(5, UserId, reason));
        Assert.Equal(StockBalance.Zero, product.Stock);
    }

    [Fact]
    public void InactiveProduct_RejectsMovementsAndEdits()
    {
        var product = new Product("SKU-1", "Produto");
        product.Deactivate();

        Assert.Throws<DomainException>(() => product.StockIn(1, UserId));
        Assert.Throws<DomainException>(() => product.UpdateDetails("Novo", null));
        Assert.Throws<DomainException>(() => product.Deactivate());
    }
}