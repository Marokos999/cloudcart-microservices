#pragma warning disable CS8619
using FluentAssertions;
using NSubstitute;
using Ordering.Application.Contracts;
using Ordering.Application.Features.Orders.Queries.GetOrdersByCustomer;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;

namespace Ordering.API.UnitTests.Features;

public class GetOrdersByCustomerHandlerTests
{
    private readonly IOrderRepository _repository;
    private readonly GetOrdersByCustomerHandler _handler;

    public GetOrdersByCustomerHandlerTests()
    {
        _repository = Substitute.For<IOrderRepository>();
        _handler = new GetOrdersByCustomerHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrders_ForGivenCustomer()
    {
        var customerId = Guid.NewGuid();
        var orders = new List<Order>
        {
            Order.Create(
                OrderId.Of(Guid.NewGuid()),
                CustomerId.Of(customerId),
                OrderName.Of("ORD_001"),
                Address.Of("Jane", "Doe", "jane@example.com", "456 Elm St", "USA", "CA", "90001"),
                Address.Of("Jane", "Doe", "jane@example.com", "456 Elm St", "USA", "CA", "90001"),
                Payment.Of("Jane Doe", "4111111111111111", "12/26", "321", 1)),
            Order.Create(
                OrderId.Of(Guid.NewGuid()),
                CustomerId.Of(customerId),
                OrderName.Of("ORD_002"),
                Address.Of("Jane", "Doe", "jane@example.com", "456 Elm St", "USA", "CA", "90001"),
                Address.Of("Jane", "Doe", "jane@example.com", "456 Elm St", "USA", "CA", "90001"),
                Payment.Of("Jane Doe", "4111111111111111", "12/26", "321", 1)),
        };
        _repository.GetOrdersByCustomerAsync(customerId, Arg.Any<CancellationToken>()).Returns(orders);

        var result = await _handler.Handle(new GetOrdersByCustomerQuery(customerId), CancellationToken.None);

        result.Orders.Should().HaveCount(2);
        result.Orders.Should().OnlyContain(o => o.CustomerId == customerId);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenCustomerHasNoOrders()
    {
        var customerId = Guid.NewGuid();
        _repository.GetOrdersByCustomerAsync(customerId, Arg.Any<CancellationToken>()).Returns([]);

        var result = await _handler.Handle(new GetOrdersByCustomerQuery(customerId), CancellationToken.None);

        result.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldNotReturnOrders_ForDifferentCustomer()
    {
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();

        _repository.GetOrdersByCustomerAsync(customerId, Arg.Any<CancellationToken>()).Returns([]);

        var result = await _handler.Handle(new GetOrdersByCustomerQuery(customerId), CancellationToken.None);

        result.Orders.Should().BeEmpty();
        await _repository.DidNotReceive().GetOrdersByCustomerAsync(otherCustomerId, Arg.Any<CancellationToken>());
    }
}
