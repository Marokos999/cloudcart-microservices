using Ordering.Application.Dtos;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(OrderDto Order) : ICommand<UpdateOrderResult>;
public record UpdateOrderResult(bool IsSuccess);

public class UpdateOrderHandler(IOrderRepository repository)
    : ICommandHandler<UpdateOrderCommand, UpdateOrderResult>
{
    public async Task<UpdateOrderResult> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await repository.GetOrderByIdAsync(command.Order.Id, cancellationToken)
            ?? throw new NotFoundException($"Order with id {command.Order.Id} not found");

        if (Enum.TryParse<OrderStatus>(command.Order.Status, out var status))
            order.UpdateStatus(status);

        await repository.SaveChangesAsync(cancellationToken);
        return new UpdateOrderResult(true);
    }
}
