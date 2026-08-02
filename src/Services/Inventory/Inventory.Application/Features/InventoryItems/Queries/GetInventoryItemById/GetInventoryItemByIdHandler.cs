using Inventory.Application.Dtos;

namespace Inventory.Application.Features.InventoryItems.Queries.GetInventoryItemById;

public record GetInventoryItemByIdQuery(Guid Id) : IQuery<GetInventoryItemByIdResult>;
public record GetInventoryItemByIdResult(InventoryItemDto Item);

public class GetInventoryItemByIdHandler(IInventoryRepository repository)
    : IQueryHandler<GetInventoryItemByIdQuery, GetInventoryItemByIdResult>
{
    public async Task<GetInventoryItemByIdResult> Handle(GetInventoryItemByIdQuery query, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException($"Inventory item with id {query.Id} not found");

        return new GetInventoryItemByIdResult(new InventoryItemDto(
            item.Id.Value,
            item.ProductId.Value,
            item.ProductName,
            item.Quantity,
            item.ReservedQuantity,
            item.AvailableQuantity));
    }
}
