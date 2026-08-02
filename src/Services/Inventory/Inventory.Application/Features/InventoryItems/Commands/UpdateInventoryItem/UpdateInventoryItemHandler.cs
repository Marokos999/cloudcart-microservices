namespace Inventory.Application.Features.InventoryItems.Commands.UpdateInventoryItem;

public record UpdateInventoryItemCommand(Guid Id, int Quantity) : ICommand<UpdateInventoryItemResult>;
public record UpdateInventoryItemResult(bool IsSuccess);

public class UpdateInventoryItemHandler(IInventoryRepository repository)
    : ICommandHandler<UpdateInventoryItemCommand, UpdateInventoryItemResult>
{
    public async Task<UpdateInventoryItemResult> Handle(UpdateInventoryItemCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException($"Inventory item with id {command.Id} not found");

        if (command.Quantity > item.Quantity)
            item.AddStock(command.Quantity - item.Quantity);
        else if (command.Quantity < item.Quantity)
            item.RemoveStock(item.Quantity - command.Quantity);

        await repository.SaveChangesAsync(cancellationToken);
        return new UpdateInventoryItemResult(true);
    }
}
