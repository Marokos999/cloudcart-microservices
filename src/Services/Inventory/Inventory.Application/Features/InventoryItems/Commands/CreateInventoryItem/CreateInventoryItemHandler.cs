using Inventory.Application.Dtos;

namespace Inventory.Application.Features.InventoryItems.Commands.CreateInventoryItem;

public record CreateInventoryItemCommand(string ProductId, string ProductName, int Quantity) : ICommand<CreateInventoryItemResult>;
public record CreateInventoryItemResult(Guid Id);

public class CreateInventoryItemHandler(IInventoryRepository repository)
    : ICommandHandler<CreateInventoryItemCommand, CreateInventoryItemResult>
{
    public async Task<CreateInventoryItemResult> Handle(CreateInventoryItemCommand command, CancellationToken cancellationToken)
    {
        var item = InventoryItem.Create(
            InventoryItemId.Of(Guid.NewGuid()),
            ProductId.Of(Guid.Parse(command.ProductId)),
            command.ProductName,
            command.Quantity);

        await repository.AddAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new CreateInventoryItemResult(item.Id.Value);
    }
}
