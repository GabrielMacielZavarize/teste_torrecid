using InventoryManagement.Application.Common;
using InventoryManagement.Application.Stock;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Abstractions;

// Sem métodos de atualização ou exclusão o historico de movimentos é imutavel
public interface IStockMovementRepository
{
    void Add(StockMovement movement);

    Task<PagedResult<StockMovementDto>> GetHistoryAsync(
        StockHistoryFilter filter,
        CancellationToken cancellationToken = default);
}
