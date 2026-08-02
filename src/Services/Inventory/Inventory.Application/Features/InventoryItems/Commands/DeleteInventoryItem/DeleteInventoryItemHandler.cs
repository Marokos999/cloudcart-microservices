namespace Inventory.Application.Features.InventoryItems.Commands.DeleteInventoryItem;

public record DeleteInventoryItemCommand(Guid Id) : ICommand<DeleteInventoryItemResult>;
public record DeleteInventoryItemResult(bool IsSuccess);

public class DeleteInventoryItemHandler(IInventoryRepository repository)
    : ICommandHandler<DeleteInventoryItemCommand, DeleteInventoryItemResult>
{
    public async Task<DeleteInventoryItemResult> Handle(DeleteInventoryItemCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException($"Inventory item with id {command.Id} not found");

        repository.Delete(item);
        await repository.SaveChangesAsync(cancellationToken);
        return new DeleteInventoryItemResult(true);
    }
}
