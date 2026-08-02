using Inventory.Application.Dtos;

namespace Inventory.Application.Features.InventoryItems.Queries.GetInventoryItemByProductId;

public record GetInventoryItemByProductIdQuery(Guid ProductId) : IQuery<GetInventoryItemByProductIdResult>;
public record GetInventoryItemByProductIdResult(InventoryItemDto Item);

public class GetInventoryItemByProductIdHandler(IInventoryRepository repository)
    : IQueryHandler<GetInventoryItemByProductIdQuery, GetInventoryItemByProductIdResult>
{
    public async Task<GetInventoryItemByProductIdResult> Handle(GetInventoryItemByProductIdQuery query, CancellationToken cancellationToken)
    {
        var item = await repository.GetByProductIdAsync(query.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Inventory item for product {query.ProductId} not found");

        return new GetInventoryItemByProductIdResult(new InventoryItemDto(
            item.Id.Value,
            item.ProductId.Value,
            item.ProductName,
            item.Quantity,
            item.ReservedQuantity,
            item.AvailableQuantity));
    }
}
