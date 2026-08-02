using Ordering.Application.Dtos;
using Ordering.Application.Extensions;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(Guid CustomerId) : IQuery<GetOrdersByCustomerResult>;
public record GetOrdersByCustomerResult(IEnumerable<OrderDto> Orders);

public class GetOrdersByCustomerHandler(IOrderRepository repository) : IQueryHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    public async Task<GetOrdersByCustomerResult> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
    {
        var orders = await repository.GetOrdersByCustomerAsync(query.CustomerId, cancellationToken);
        return new GetOrdersByCustomerResult(orders.ToDtoList());
    }
}
