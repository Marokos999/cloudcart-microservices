using Ordering.Application.Dtos;
using Ordering.Application.Extensions;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQuery() : IQuery<GetOrdersResult>;
public record GetOrdersResult(IEnumerable<OrderDto> Orders);

public class GetOrdersHandler(IOrderRepository repository) : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = await repository.GetOrdersAsync(cancellationToken);
        return new GetOrdersResult(orders.ToDtoList());
    }
}
