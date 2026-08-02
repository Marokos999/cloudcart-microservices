using Inventory.Application.Dtos;

namespace Inventory.Application.Features.InventoryItems.Queries.GetInventoryItems;

public record GetInventoryItemsQuery() : IQuery<GetInventoryItemsResult>;
public record GetInventoryItemsResult(IEnumerable<InventoryItemDto> Items);

public class GetInventoryItemsHandler(IInventoryRepository repository)
    : IQueryHandler<GetInventoryItemsQuery, GetInventoryItemsResult>
{
    public async Task<GetInventoryItemsResult> Handle(GetInventoryItemsQuery query, CancellationToken cancellationToken)
    {
        var items = await repository.GetAllAsync(cancellationToken);
        return new GetInventoryItemsResult(items.Select(i => new InventoryItemDto(
            i.Id.Value,
            i.ProductId.Value,
            i.ProductName,
            i.Quantity,
            i.ReservedQuantity,
            i.AvailableQuantity)));
    }
}
