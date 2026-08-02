using Ordering.Application.Dtos;
using Ordering.Application.Extensions;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IQuery<GetOrderByIdResult>;
public record GetOrderByIdResult(OrderDto Order);

public class GetOrderByIdHandler(IOrderRepository repository)
    : IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    public async Task<GetOrderByIdResult> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await repository.GetOrderByIdAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException($"Order with id {query.Id} not found");

        return new GetOrderByIdResult(order.ToDto());
    }
}
