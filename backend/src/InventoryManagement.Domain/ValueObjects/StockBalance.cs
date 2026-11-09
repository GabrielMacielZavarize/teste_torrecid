using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Domain.ValueObjects;

public sealed record StockBalance
{
    public static StockBalance Zero => new(0, 0);

    public int Physical { get; }
    public int Reserved { get; }
    public int Available => Physical - Reserved;

    public StockBalance(int physical, int reserved)
    {
        if (physical < 0)
            throw new DomainException("O saldo físico não pode ser negativo.");
        if (reserved < 0)
            throw new DomainException("O saldo reservado não pode ser negativo.");
        if (reserved > physical)
            throw new DomainException("O saldo reservado não pode ser maior que o físico.");

        Physical = physical;
        Reserved = reserved;
    }

    public StockBalance StockIn(int quantity)
    {
        EnsurePositive(quantity);
        return new StockBalance(Physical + quantity, Reserved);
    }

    public StockBalance StockOut(int quantity)
    {
        EnsurePositive(quantity);
        EnsureAvailable(quantity);
        return new StockBalance(Physical - quantity, Reserved);
    }

    public StockBalance Reserve(int quantity)
    {
        EnsurePositive(quantity);
        EnsureAvailable(quantity);
        return new StockBalance(Physical, Reserved + quantity);
    }

    public StockBalance CancelReservation(int quantity)
    {
        EnsurePositive(quantity);
        EnsureReserved(quantity);
        return new StockBalance(Physical, Reserved - quantity);
    }

    public StockBalance FulfillReservation(int quantity)
    {
        EnsurePositive(quantity);
        EnsureReserved(quantity);
        return new StockBalance(Physical - quantity, Reserved - quantity);
    }

    public StockBalance AdjustTo(int countedQuantity)
    {
        if (countedQuantity < 0)
            throw new DomainException("A quantidade contada não pode ser negativa.");
        if (countedQuantity < Reserved)
            throw new DomainException("A quantidade contada não pode ficar abaixo do saldo reservado.");

        return new StockBalance(countedQuantity, Reserved);
    }

    private static void EnsurePositive(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");
    }

    private void EnsureAvailable(int quantity)
    {
        if (quantity > Available)
            throw new DomainException("Saldo disponível insuficiente.");
    }

    private void EnsureReserved(int quantity)
    {
        if (quantity > Reserved)
            throw new DomainException("Saldo reservado insuficiente.");
    }
}