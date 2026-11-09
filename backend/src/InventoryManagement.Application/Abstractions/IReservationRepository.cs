using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.Abstractions;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reservation>> ListAsync(
        Guid? productId,
        ReservationStatus? status,
        CancellationToken cancellationToken = default);

    void Add(Reservation reservation);
}
