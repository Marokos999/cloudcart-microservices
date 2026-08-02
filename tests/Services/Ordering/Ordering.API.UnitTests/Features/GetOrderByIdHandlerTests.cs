#pragma warning disable CS8619
using CloudCart.BuildingBlocks.Exceptions;
using FluentAssertions;
using NSubstitute;
using Ordering.Application.Contracts;
using Ordering.Application.Features.Orders.Queries.GetOrderById;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;

namespace Ordering.API.UnitTests.Features;

public class GetOrderByIdHandlerTests
{
    private readonly IOrderRepository _repository;
    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _repository = Substitute.For<IOrderRepository>();
        _handler = new GetOrderByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrder_WhenExists()
    {
        var orderId = Guid.NewGuid();
        var order = Order.Create(
            OrderId.Of(orderId),
            CustomerId.Of(Guid.NewGuid()),
            OrderName.Of("ORD_001"),
            Address.Of("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
            Address.Of("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
            Payment.Of("John Doe", "4111111111111111", "12/26", "123", 1));

        _repository.GetOrderByIdAsync(orderId, Arg.Any<CancellationToken>()).Returns(order);

        var result = await _handler.Handle(new GetOrderByIdQuery(orderId), CancellationToken.None);

        result.Order.Should().NotBeNull();
        result.Order.OrderName.Should().Be("ORD_001");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();
        _repository.GetOrderByIdAsync(orderId, Arg.Any<CancellationToken>()).Returns((Order?)null);

        var act = async () => await _handler.Handle(new GetOrderByIdQuery(orderId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{orderId}*");
    }
}
