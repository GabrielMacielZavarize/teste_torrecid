using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.ValueObjects;

namespace InventoryManagement.Domain.Tests.ValueObjects;

public class StockBalanceTests
{
    [Fact]
    public void Constructor_WithReservedAbovePhysical_Throws()
    {
        Assert.Throws<DomainException>(() => new StockBalance(5, 6));
    }

    [Fact]
    public void StockIn_ReturnsNewBalance_AndKeepsOriginalUnchanged()
    {
        var original = new StockBalance(10, 2);

        var result = original.StockIn(5);

        Assert.Equal(new StockBalance(15, 2), result);
        Assert.Equal(new StockBalance(10, 2), original);
    }

    [Fact]
    public void StockOut_AboveAvailable_Throws()
    {
        var balance = new StockBalance(10, 2);

        Assert.Throws<DomainException>(() => balance.StockOut(9));
    }

    [Fact]
    public void Reserve_AboveAvailable_Throws()
    {
        var balance = new StockBalance(10, 2);

        Assert.Throws<DomainException>(() => balance.Reserve(9));
    }

    [Fact]
    public void FulfillReservation_DecreasesPhysicalAndReserved()
    {
        var result = new StockBalance(10, 4).FulfillReservation(4);

        Assert.Equal(new StockBalance(6, 0), result);
    }

    [Fact]
    public void AdjustTo_BelowReserved_Throws()
    {
        var balance = new StockBalance(10, 4);

        Assert.Throws<DomainException>(() => balance.AdjustTo(3));
    }
}
