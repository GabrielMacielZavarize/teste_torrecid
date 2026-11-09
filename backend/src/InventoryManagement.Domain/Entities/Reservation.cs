using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Domain.Entities;

public class Reservation
{
    public Guid Id { get; private init; }
    public Guid ProductId { get; private init; }
    public int Quantity { get; private init; }
    public ReservationStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private init; }
    public DateTime CreatedAt { get; private init; }

    internal Reservation(Guid productId, int quantity, Guid createdByUserId)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
        Status = ReservationStatus.Active;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    internal void Cancel()
    {
        EnsureActive();
        Status = ReservationStatus.Cancelled;
    }

    internal void Fulfill()
    {
        EnsureActive();
        Status = ReservationStatus.Fulfilled;
    }

    private void EnsureActive()
    {
        if (Status != ReservationStatus.Active)
            throw new DomainException("Somente reservas ativas podem ser canceladas ou atendidas.");
    }
}