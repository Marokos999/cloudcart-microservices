#pragma warning disable CS8619
using FluentAssertions;
using NSubstitute;
using Ordering.Application.Contracts;
using Ordering.Application.Dtos;
using Ordering.Application.Features.Orders.Commands.CreateOrder;

namespace Ordering.API.UnitTests.Features;

public class CreateOrderHandlerTests
{
    private readonly IOrderRepository _repository;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _repository = Substitute.For<IOrderRepository>();
        _handler = new CreateOrderHandler(_repository);
    }

    private static OrderDto BuildOrderDto(string orderName = "ORD_001") => new(
        Id: Guid.NewGuid(),
        CustomerId: Guid.NewGuid(),
        OrderName: orderName,
        ShippingAddress: new AddressDto("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
        BillingAddress: new AddressDto("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
        Payment: new PaymentDto("John Doe", "4111111111111111", "12/26", "123", 1),
        Status: "Pending",
        OrderItems: [new OrderItemDto(Guid.NewGuid(), Guid.NewGuid(), 2, 499.99m)],
        TotalPrice: 999.98m,
        CreatedAt: DateTime.UtcNow);

    [Fact]
    public async Task Handle_ShouldCreateOrder_AndReturnId()
    {
        var dto = BuildOrderDto();
        _repository.AddAsync(Arg.Any<Ordering.Domain.Models.Order>(), Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult(x.Arg<Ordering.Domain.Models.Order>()));

        var result = await _handler.Handle(new CreateOrderCommand(dto), CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsync_Once()
    {
        var dto = BuildOrderDto("ORD_002");
        _repository.AddAsync(Arg.Any<Ordering.Domain.Models.Order>(), Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult(x.Arg<Ordering.Domain.Models.Order>()));

        await _handler.Handle(new CreateOrderCommand(dto), CancellationToken.None);

        await _repository.Received(1).AddAsync(Arg.Any<Ordering.Domain.Models.Order>(), Arg.Any<CancellationToken>());
    }
}
