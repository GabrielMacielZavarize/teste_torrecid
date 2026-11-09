using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Common;
using InventoryManagement.Application.Exceptions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.Stock;

public sealed class StockService(
    IProductRepository products,
    IReservationRepository reservations,
    IStockMovementRepository movements,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
{
    private const int MaxPageSize = 100;

    public Task<StockMovementDto> StockInAsync(
        Guid productId,
        StockOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            productId,
            product => product.StockIn(request.Quantity, currentUser.UserId, request.Reason),
            cancellationToken);
    }

    public Task<StockMovementDto> StockOutAsync(
        Guid productId,
        StockOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            productId,
            product => product.StockOut(request.Quantity, currentUser.UserId, request.Reason),
            cancellationToken);
    }

    public Task<StockMovementDto> AdjustAsync(
        Guid productId,
        AdjustStockRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            productId,
            product => product.AdjustStock(request.CountedQuantity, currentUser.UserId, request.Reason),
            cancellationToken);
    }

    public async Task<ReservationDto> ReserveAsync(
        Guid productId,
        StockOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductAsync(productId, cancellationToken);

        var (reservation, movement) = product.Reserve(request.Quantity, currentUser.UserId, request.Reason);

        reservations.Add(reservation);
        movements.Add(movement);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationDto.From(reservation);
    }

    public Task<StockMovementDto> CancelReservationAsync(
        Guid reservationId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        return ExecuteOnReservationAsync(
            reservationId,
            (product, reservation) => product.CancelReservation(reservation, currentUser.UserId, reason),
            cancellationToken);
    }

    public Task<StockMovementDto> FulfillReservationAsync(
        Guid reservationId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        return ExecuteOnReservationAsync(
            reservationId,
            (product, reservation) => product.FulfillReservation(reservation, currentUser.UserId, reason),
            cancellationToken);
    }

    public async Task<IReadOnlyList<ReservationDto>> ListReservationsAsync(
        Guid? productId,
        ReservationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var items = await reservations.ListAsync(productId, status, cancellationToken);
        return items.Select(ReservationDto.From).ToList();
    }

    public Task<PagedResult<StockMovementDto>> GetHistoryAsync(
        StockHistoryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var normalizedFilter = filter with
        {
            Page = Math.Max(filter.Page, 1),
            PageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize)
        };

        return movements.GetHistoryAsync(normalizedFilter, cancellationToken);
    }

    private async Task<StockMovementDto> ExecuteAsync(
        Guid productId,
        Func<Product, StockMovement> operation,
        CancellationToken cancellationToken)
    {
        var product = await GetProductAsync(productId, cancellationToken);

        var movement = operation(product);

        movements.Add(movement);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return StockMovementDto.From(movement, product);
    }

    private async Task<StockMovementDto> ExecuteOnReservationAsync(
        Guid reservationId,
        Func<Product, Reservation, StockMovement> operation,
        CancellationToken cancellationToken)
    {
        var reservation = await reservations.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new NotFoundException("Reserva não encontrada.");
        var product = await GetProductAsync(reservation.ProductId, cancellationToken);

        var movement = operation(product, reservation);

        movements.Add(movement);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return StockMovementDto.From(movement, product);
    }

    private async Task<Product> GetProductAsync(Guid id, CancellationToken cancellationToken)
    {
        return await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Produto não encontrado.");
    }
}
