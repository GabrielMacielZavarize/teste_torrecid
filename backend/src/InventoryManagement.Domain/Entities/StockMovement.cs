using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.ValueObjects;

namespace InventoryManagement.Domain.Entities;

public class StockMovement
{
    public const int ReasonMaxLength = 500;

    public Guid Id { get; private init; }
    public Guid ProductId { get; private init; }
    public Guid? ReservationId { get; private init; }
    public MovementType Type { get; private init; }
    public int Quantity { get; private init; }
    public StockBalance BalanceBefore { get; private init; }
    public StockBalance BalanceAfter { get; private init; }
    public Guid UserId { get; private init; }
    public DateTime OccurredAt { get; private init; }
    public string? Reason { get; private init; }

    private StockMovement()
    {
        BalanceBefore = null!;
        BalanceAfter = null!;
    }

    internal StockMovement(
        Guid productId,
        MovementType type,
        int quantity,
        StockBalance balanceBefore,
        StockBalance balanceAfter,
        Guid userId,
        string? reason,
        Guid? reservationId)
    {
        var normalizedReason = reason?.Trim();
        if (normalizedReason?.Length > ReasonMaxLength)
            throw new DomainException($"O motivo aceita no máximo {ReasonMaxLength} caracteres.");

        if (userId == Guid.Empty)
            throw new DomainException("Todo movimento precisa de um usuário responsável.");

        Id = Guid.NewGuid();
        ProductId = productId;
        ReservationId = reservationId;
        Type = type;
        Quantity = quantity;
        BalanceBefore = balanceBefore;
        BalanceAfter = balanceAfter;
        UserId = userId;
        OccurredAt = DateTime.UtcNow;
        Reason = normalizedReason;
    }
}