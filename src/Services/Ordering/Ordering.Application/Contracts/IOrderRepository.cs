using Ordering.Domain.Models;

namespace Ordering.Application.Contracts;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    void Delete(Order order);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}