using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.ValueObjects;

namespace InventoryManagement.Domain.Entities;

public class Product
{
    public const int SkuMaxLength = 50;
    public const int NameMaxLength = 150;
    public const int DescriptionMaxLength = 500;

    public Guid Id { get; private init; }
    public string Sku { get; private init; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public StockBalance Stock { get; private set; }

    public Product(string sku, string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Sku = NormalizeRequired(sku, "SKU", SkuMaxLength).ToUpperInvariant();
        Name = NormalizeRequired(name, "nome", NameMaxLength);
        Description = NormalizeOptional(description, "descrição", DescriptionMaxLength);
        IsActive = true;
        Stock = StockBalance.Zero;
    }

    public void UpdateDetails(string name, string? description)
    {
        EnsureActive();
        Name = NormalizeRequired(name, "nome", NameMaxLength);
        Description = NormalizeOptional(description, "descrição", DescriptionMaxLength);
    }

    public void Deactivate()
    {
        EnsureActive();

        // Reservado nunca passa do físico: físico zero garante que não há reserva ativa.
        if (Stock.Physical != 0)
            throw new DomainException("Só é possível excluir um produto com saldo físico zerado.");

        IsActive = false;
    }

    public StockMovement StockIn(int quantity, Guid userId, string? reason = null)
    {
        EnsureActive();
        return ApplyMovement(MovementType.StockIn, quantity, Stock.StockIn(quantity), userId, reason);
    }

    public StockMovement StockOut(int quantity, Guid userId, string? reason = null)
    {
        EnsureActive();
        return ApplyMovement(MovementType.StockOut, quantity, Stock.StockOut(quantity), userId, reason);
    }

    public (Reservation Reservation, StockMovement Movement) Reserve(
        int quantity,
        Guid userId,
        string? reason = null)
    {
        EnsureActive();

        var newBalance = Stock.Reserve(quantity);
        var reservation = new Reservation(Id, quantity, userId);
        var movement = ApplyMovement(
            MovementType.Reservation, quantity, newBalance, userId, reason, reservation.Id);

        return (reservation, movement);
    }

    public StockMovement CancelReservation(Reservation reservation, Guid userId, string? reason = null)
    {
        EnsureActive();
        EnsureBelongsToThisProduct(reservation);

        reservation.Cancel();

        return ApplyMovement(
            MovementType.ReservationCancellation,
            reservation.Quantity,
            Stock.CancelReservation(reservation.Quantity),
            userId,
            reason,
            reservation.Id);
    }

    public StockMovement FulfillReservation(Reservation reservation, Guid userId, string? reason = null)
    {
        EnsureActive();
        EnsureBelongsToThisProduct(reservation);

        reservation.Fulfill();

        return ApplyMovement(
            MovementType.ReservationFulfillment,
            reservation.Quantity,
            Stock.FulfillReservation(reservation.Quantity),
            userId,
            reason,
            reservation.Id);
    }

    public StockMovement AdjustStock(int countedQuantity, Guid userId, string reason)
    {
        EnsureActive();

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("O motivo é obrigatório no ajuste manual.");

        var newBalance = Stock.AdjustTo(countedQuantity);
        var difference = newBalance.Physical - Stock.Physical;

        return ApplyMovement(MovementType.Adjustment, difference, newBalance, userId, reason);
    }

    private StockMovement ApplyMovement(
        MovementType type,
        int quantity,
        StockBalance newBalance,
        Guid userId,
        string? reason,
        Guid? reservationId = null)
    {
        var movement = new StockMovement(Id, type, quantity, Stock, newBalance, userId, reason, reservationId);
        Stock = newBalance;
        return movement;
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException("O produto está inativo.");
    }

    private void EnsureBelongsToThisProduct(Reservation reservation)
    {
        if (reservation.ProductId != Id)
            throw new DomainException("A reserva não pertence a este produto.");
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"O campo {fieldName} é obrigatório.");

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException($"O campo {fieldName} aceita no máximo {maxLength} caracteres.");

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, string fieldName, int maxLength)
    {
        return string.IsNullOrWhiteSpace(value) ? null : NormalizeRequired(value, fieldName, maxLength);
    }
}