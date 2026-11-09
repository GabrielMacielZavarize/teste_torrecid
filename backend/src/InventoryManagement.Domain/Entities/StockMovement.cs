using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.ValueObjects;

namespace InventoryManagement.Domain.Entities;

public class StockMovement
{
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
        Reason = reason?.Trim();
    }
}