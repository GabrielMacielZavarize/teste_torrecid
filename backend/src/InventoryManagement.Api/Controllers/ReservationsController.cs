using InventoryManagement.Api.Authorization;
using InventoryManagement.Application.Stock;
using InventoryManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InventoryManagement.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize(Policy = Permissions.StockMove)]
public sealed class ReservationsController(StockService stockService) : ControllerBase
{
    [HttpGet("reservations")]
    public Task<IReadOnlyList<ReservationDto>> List(
        [FromQuery] Guid? productId,
        [FromQuery] ReservationStatus? status,
        CancellationToken cancellationToken)
    {
        return stockService.ListReservationsAsync(productId, status, cancellationToken);
    }

    [HttpPost("products/{productId:guid}/reservations")]
    public Task<ReservationDto> Reserve(
        Guid productId,
        StockOperationRequest request,
        CancellationToken cancellationToken)
    {
        return stockService.ReserveAsync(productId, request, cancellationToken);
    }

    [HttpPost("reservations/{id:guid}/cancel")]
    public Task<StockMovementDto> Cancel(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ReservationActionRequest? request,
        CancellationToken cancellationToken)
    {
        return stockService.CancelReservationAsync(id, request?.Reason, cancellationToken);
    }

    [HttpPost("reservations/{id:guid}/fulfill")]
    public Task<StockMovementDto> Fulfill(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ReservationActionRequest? request,
        CancellationToken cancellationToken)
    {
        return stockService.FulfillReservationAsync(id, request?.Reason, cancellationToken);
    }
}
