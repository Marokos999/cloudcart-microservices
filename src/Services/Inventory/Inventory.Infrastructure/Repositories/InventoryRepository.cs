using Inventory.Application.Contracts;
using Inventory.Domain.Models;
using Inventory.Domain.ValueObjects;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class InventoryRepository(InventoryContext context) : IInventoryRepository
{
    public async Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.InventoryItems.ToListAsync(cancellationToken);

    public async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.InventoryItems.FindAsync([InventoryItemId.Of(id)], cancellationToken);

    public async Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        => await context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == ProductId.Of(productId), cancellationToken);

    public async Task<InventoryItem> AddAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        await context.InventoryItems.AddAsync(item, cancellationToken);
        return item;
    }

    public void Delete(InventoryItem item)
        => context.InventoryItems.Remove(item);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
