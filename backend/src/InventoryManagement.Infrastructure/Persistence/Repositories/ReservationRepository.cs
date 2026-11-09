using InventoryManagement.Application.Abstractions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Persistence.Repositories;

internal sealed class ReservationRepository(AppDbContext context) : IReservationRepository
{
    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Reservations.FirstOrDefaultAsync(reservation => reservation.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> ListAsync(
        Guid? productId,
        ReservationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = context.Reservations.AsNoTracking();

        if (productId is not null)
            query = query.Where(reservation => reservation.ProductId == productId);

        if (status is not null)
            query = query.Where(reservation => reservation.Status == status);

        return await query
            .OrderByDescending(reservation => reservation.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(Reservation reservation)
    {
        context.Reservations.Add(reservation);
    }
}
