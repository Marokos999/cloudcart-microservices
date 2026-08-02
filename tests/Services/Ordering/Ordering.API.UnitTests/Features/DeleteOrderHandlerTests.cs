using FluentAssertions;
using NSubstitute;
using Ordering.Application.Contracts;
using Ordering.Application.Features.Orders.Commands.DeleteOrder;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;

namespace Ordering.API.UnitTests.Features;

public class DeleteOrderHandlerTests
{
    private readonly IOrderRepository _repository;
    private readonly DeleteOrderHandler _handler;

    public DeleteOrderHandlerTests()
    {
        _repository = Substitute.For<IOrderRepository>();
        _handler = new DeleteOrderHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldDeleteOrder_AndReturnSuccess()
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

        var result = await _handler.Handle(new DeleteOrderCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repository.Received(1).Delete(order);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();
        _repository.GetOrderByIdAsync(orderId, Arg.Any<CancellationToken>()).Returns((Order?)null);

        var act = async () => await _handler.Handle(new DeleteOrderCommand(orderId), CancellationToken.None);

        await act.Should().ThrowAsync<CloudCart.BuildingBlocks.Exceptions.NotFoundException>();
    }
}
