using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Common;
using InventoryManagement.Application.Stock;
using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Persistence.Repositories;

internal sealed class StockMovementRepository(AppDbContext context) : IStockMovementRepository
{
    public void Add(StockMovement movement)
    {
        context.StockMovements.Add(movement);
    }

    public async Task<PagedResult<StockMovementDto>> GetHistoryAsync(
        StockHistoryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var movements = context.StockMovements.AsNoTracking();

        if (filter.ProductId is not null)
            movements = movements.Where(movement => movement.ProductId == filter.ProductId);

        var totalCount = await movements.CountAsync(cancellationToken);

        // O join não filtra por IsActive produto inativo continua aparecendo no histórico.
        var items = await (
                from movement in movements
                join product in context.Products.AsNoTracking() on movement.ProductId equals product.Id
                join user in context.Users.AsNoTracking() on movement.UserId equals user.Id
                orderby movement.OccurredAt descending, movement.Id
                select new StockMovementDto(
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
                    user.Username,
                    movement.OccurredAt,
                    movement.Reason))
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<StockMovementDto>(items, totalCount, filter.Page, filter.PageSize);
    }
}
