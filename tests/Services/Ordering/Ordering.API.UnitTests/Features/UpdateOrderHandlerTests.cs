using FluentAssertions;
using NSubstitute;
using Ordering.Application.Contracts;
using Ordering.Application.Dtos;
using Ordering.Application.Features.Orders.Commands.UpdateOrder;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;

namespace Ordering.API.UnitTests.Features;

public class UpdateOrderHandlerTests
{
    private readonly IOrderRepository _repository;
    private readonly UpdateOrderHandler _handler;

    public UpdateOrderHandlerTests()
    {
        _repository = Substitute.For<IOrderRepository>();
        _handler = new UpdateOrderHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatus_AndReturnSuccess()
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

        var dto = new OrderDto(orderId, Guid.NewGuid(), "ORD_001",
            new AddressDto("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
            new AddressDto("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
            new PaymentDto("John Doe", "4111111111111111", "12/26", "123", 1),
            "Processing", [], 0m, DateTime.UtcNow);

        var result = await _handler.Handle(new UpdateOrderCommand(dto), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Processing);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();
        _repository.GetOrderByIdAsync(orderId, Arg.Any<CancellationToken>()).Returns((Order?)null);

        var dto = new OrderDto(orderId, Guid.NewGuid(), "ORD_001",
            new AddressDto("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
            new AddressDto("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
            new PaymentDto("John Doe", "4111111111111111", "12/26", "123", 1),
            "Processing", [], 0m, DateTime.UtcNow);

        var act = async () => await _handler.Handle(new UpdateOrderCommand(dto), CancellationToken.None);

        await act.Should().ThrowAsync<CloudCart.BuildingBlocks.Exceptions.NotFoundException>();
    }
}
