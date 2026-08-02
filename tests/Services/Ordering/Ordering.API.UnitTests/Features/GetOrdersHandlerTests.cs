using FluentAssertions;
using NSubstitute;
using Ordering.Application.Contracts;
using Ordering.Application.Features.Orders.Queries.GetOrders;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;

namespace Ordering.API.UnitTests.Features;

public class GetOrdersHandlerTests
{
    private readonly IOrderRepository _repository;
    private readonly GetOrdersHandler _handler;

    public GetOrdersHandlerTests()
    {
        _repository = Substitute.For<IOrderRepository>();
        _handler = new GetOrdersHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllOrders()
    {
        var orders = new List<Order>
        {
            Order.Create(
                OrderId.Of(Guid.NewGuid()),
                CustomerId.Of(Guid.NewGuid()),
                OrderName.Of("ORD_001"),
                Address.Of("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
                Address.Of("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
                Payment.Of("John Doe", "4111111111111111", "12/26", "123", 1))
        };
        _repository.GetOrdersAsync(Arg.Any<CancellationToken>()).Returns(orders);

        var result = await _handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        result.Orders.Should().HaveCount(1);
        result.Orders.First().OrderName.Should().Be("ORD_001");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoOrders()
    {
        _repository.GetOrdersAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await _handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        result.Orders.Should().BeEmpty();
    }
}
