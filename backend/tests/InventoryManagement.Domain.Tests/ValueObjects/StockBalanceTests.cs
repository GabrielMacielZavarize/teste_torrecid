using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.ValueObjects;

namespace InventoryManagement.Domain.Tests.ValueObjects;

public class StockBalanceTests
{
    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(5, 6)]
    public void Constructor_WithInvalidValues_Throws(int physical, int reserved)
    {
        Assert.Throws<DomainException>(() => new StockBalance(physical, reserved));
    }

    [Fact]
    public void Constructor_WithValidValues_CalculatesAvailable()
    {
        var balance = new StockBalance(10, 4);

        Assert.Equal(6, balance.Available);
    }

    [Fact]
    public void Equality_WithSameValues_IsEqual()
    {
        Assert.Equal(new StockBalance(10, 2), new StockBalance(10, 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Operations_WithNonPositiveQuantity_Throw(int quantity)
    {
        var balance = new StockBalance(10, 4);

        Assert.Throws<DomainException>(() => balance.StockIn(quantity));
        Assert.Throws<DomainException>(() => balance.StockOut(quantity));
        Assert.Throws<DomainException>(() => balance.Reserve(quantity));
        Assert.Throws<DomainException>(() => balance.CancelReservation(quantity));
        Assert.Throws<DomainException>(() => balance.FulfillReservation(quantity));
    }

    [Fact]
    public void StockIn_IncreasesPhysical_AndKeepsOriginalUnchanged()
    {
        var original = new StockBalance(10, 2);

        var result = original.StockIn(5);

        Assert.Equal(new StockBalance(15, 2), result);
        Assert.Equal(new StockBalance(10, 2), original);
    }

    [Fact]
    public void StockOut_ExactlyAvailable_Works()
    {
        var result = new StockBalance(10, 2).StockOut(8);

        Assert.Equal(new StockBalance(2, 2), result);
    }

    [Fact]
    public void StockOut_AboveAvailable_Throws()
    {
        var balance = new StockBalance(10, 2);

        Assert.Throws<DomainException>(() => balance.StockOut(9));
    }

    [Fact]
    public void Reserve_ExactlyAvailable_Works()
    {
        var result = new StockBalance(10, 2).Reserve(8);

        Assert.Equal(new StockBalance(10, 10), result);
    }

    [Fact]
    public void Reserve_AboveAvailable_Throws()
    {
        var balance = new StockBalance(10, 2);

        Assert.Throws<DomainException>(() => balance.Reserve(9));
    }

    [Fact]
    public void CancelReservation_ExactlyReserved_ReleasesOnlyReserved()
    {
        var result = new StockBalance(10, 4).CancelReservation(4);

        Assert.Equal(new StockBalance(10, 0), result);
    }

    [Fact]
    public void CancelReservation_AboveReserved_Throws()
    {
        var balance = new StockBalance(10, 4);

        Assert.Throws<DomainException>(() => balance.CancelReservation(5));
    }

    [Fact]
    public void FulfillReservation_DecreasesPhysicalAndReserved()
    {
        var result = new StockBalance(10, 4).FulfillReservation(4);

        Assert.Equal(new StockBalance(6, 0), result);
    }

    [Fact]
    public void FulfillReservation_AboveReserved_Throws()
    {
        var balance = new StockBalance(10, 4);

        Assert.Throws<DomainException>(() => balance.FulfillReservation(5));
    }

    [Fact]
    public void AdjustTo_EqualToReserved_Works()
    {
        var result = new StockBalance(10, 4).AdjustTo(4);

        Assert.Equal(new StockBalance(4, 4), result);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(-1)]
    public void AdjustTo_BelowReservedOrNegative_Throws(int counted)
    {
        var balance = new StockBalance(10, 4);

        Assert.Throws<DomainException>(() => balance.AdjustTo(counted));
    }
}