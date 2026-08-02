namespace Inventory.Application.Contracts;

public interface IInventoryRepository
{
    Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<InventoryItem> AddAsync(InventoryItem item, CancellationToken cancellationToken = default);
    void Delete(InventoryItem item);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}