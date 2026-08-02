using CloudCart.BuildingBlocks.Events;
using Inventory.Application.Contracts;
using MassTransit;

namespace Inventory.API.EventHandlers;

public class BasketCheckoutEventHandler(
    IInventoryRepository repository,
    ILogger<BasketCheckoutEventHandler> logger) : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        foreach (var item in context.Message.Items)
        {
            if (!Guid.TryParse(item.ProductId, out var productId))
            {
                logger.LogWarning("Skipping stock update — invalid product id {ProductId}", item.ProductId);
                continue;
            }

            var inventoryItem = await repository.GetByProductIdAsync(productId, context.CancellationToken);
            if (inventoryItem is null)
            {
                logger.LogWarning("No inventory record for product {ProductName} ({ProductId})",
                    item.ProductName, item.ProductId);
                continue;
            }

            try
            {
                inventoryItem.Reserve(item.Quantity);
                logger.LogInformation("Stock reserved: {ProductName} -{Quantity} available (reserved: {Reserved})",
                    item.ProductName, item.Quantity, inventoryItem.ReservedQuantity);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Could not reduce stock for {ProductName}: {Message}",
                    item.ProductName, ex.Message);
            }
        }

        await repository.SaveChangesAsync(context.CancellationToken);
    }
}
