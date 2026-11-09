using InventoryManagement.Api.Authorization;
using InventoryManagement.Application.Common;
using InventoryManagement.Application.Stock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class StockController(StockService stockService) : ControllerBase
{
    [HttpPost("products/{productId:guid}/stock/in")]
    [Authorize(Policy = Permissions.StockMove)]
    public Task<StockMovementDto> StockIn(
        Guid productId,
        StockOperationRequest request,
        CancellationToken cancellationToken)
    {
        return stockService.StockInAsync(productId, request, cancellationToken);
    }

    [HttpPost("products/{productId:guid}/stock/out")]
    [Authorize(Policy = Permissions.StockMove)]
    public Task<StockMovementDto> StockOut(
        Guid productId,
        StockOperationRequest request,
        CancellationToken cancellationToken)
    {
        return stockService.StockOutAsync(productId, request, cancellationToken);
    }

    [HttpPost("products/{productId:guid}/stock/adjust")]
    [Authorize(Policy = Permissions.StockMove)]
    public Task<StockMovementDto> Adjust(
        Guid productId,
        AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        return stockService.AdjustAsync(productId, request, cancellationToken);
    }

    [HttpGet("stock/movements")]
    [Authorize(Policy = Permissions.StockViewHistory)]
    public Task<PagedResult<StockMovementDto>> GetHistory(
        [FromQuery] Guid? productId,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return stockService.GetHistoryAsync(new StockHistoryFilter(productId, page, pageSize), cancellationToken);
    }
}
