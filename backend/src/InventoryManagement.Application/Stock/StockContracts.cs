using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.Stock;

public sealed record StockOperationRequest(int Quantity, string? Reason);

public sealed record AdjustStockRequest(int CountedQuantity, string Reason);

public sealed record ReservationActionRequest(string? Reason);

public sealed record StockHistoryFilter(Guid? ProductId = null, int Page = 1, int PageSize = 20);

public sealed record ReservationDto(
    Guid Id,
    Guid ProductId,
    int Quantity,
    ReservationStatus Status,
    Guid CreatedByUserId,
    DateTime CreatedAt)
{
    public static ReservationDto From(Reservation reservation) => new(
        reservation.Id,
        reservation.ProductId,
        reservation.Quantity,
        reservation.Status,
        reservation.CreatedByUserId,
        reservation.CreatedAt);
}

public sealed record StockMovementDto(
    Guid Id,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    Guid? ReservationId,
    MovementType Type,
    int Quantity,
    int PhysicalBefore,
    int ReservedBefore,
    int PhysicalAfter,
    int ReservedAfter,
    Guid UserId,
    string Username,
    DateTime OccurredAt,
    string? Reason)
{
    public static StockMovementDto From(StockMovement movement, Product product, string username) => new(
        movement.Id,
        movement.ProductId,
        product.Sku,
        product.Name,
        movement.ReservationId,
        movement.Type,
        movement.Quantity,
        movement.BalanceBefore.Physical,
        movement.BalanceBefore.Reserved,
        movement.BalanceAfter.Physical,
        movement.BalanceAfter.Reserved,
        movement.UserId,
        username,
        movement.OccurredAt,
        movement.Reason);
}
