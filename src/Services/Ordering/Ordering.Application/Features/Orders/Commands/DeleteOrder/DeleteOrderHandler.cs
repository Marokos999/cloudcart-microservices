namespace Ordering.Application.Features.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(Guid Id) : ICommand<DeleteOrderResult>;
public record DeleteOrderResult(bool IsSuccess);

public class DeleteOrderHandler(IOrderRepository repository)
    : ICommandHandler<DeleteOrderCommand, DeleteOrderResult>
{
    public async Task<DeleteOrderResult> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await repository.GetOrderByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException($"Order with id {command.Id} not found");

        repository.Delete(order);
        await repository.SaveChangesAsync(cancellationToken);
        return new DeleteOrderResult(true);
    }
}
