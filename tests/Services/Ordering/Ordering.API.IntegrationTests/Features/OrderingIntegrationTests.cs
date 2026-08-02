using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Commands.CreateOrder;
using Ordering.Application.Features.Orders.Commands.DeleteOrder;
using Ordering.Application.Features.Orders.Commands.UpdateOrder;
using Ordering.Application.Features.Orders.Queries.GetOrders;
using Ordering.Application.Features.Orders.Queries.GetOrdersByCustomer;
using Ordering.Application.Dtos;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;
using Ordering.Infrastructure.Data;
using Ordering.Infrastructure.Repositories;
using Testcontainers.MsSql;

namespace Ordering.API.IntegrationTests.Features;

public class OrderingIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private OrderingContext _context = null!;
    private OrderRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<OrderingContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .Options;

        _context = new OrderingContext(options);
        await _context.Database.MigrateAsync();
        _repository = new OrderRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _sqlContainer.StopAsync();
    }

    private static Order BuildOrder(string orderName = "ORD_001") =>
        Order.Create(
            OrderId.Of(Guid.NewGuid()),
            CustomerId.Of(Guid.NewGuid()),
            OrderName.Of(orderName),
            Address.Of("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
            Address.Of("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
            Payment.Of("John Doe", "4111111111111111", "12/26", "123", 1));

    private static OrderDto BuildOrderDto(string orderName = "ORD_001") => new(
        Id: Guid.NewGuid(),
        CustomerId: Guid.NewGuid(),
        OrderName: orderName,
        ShippingAddress: new AddressDto("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
        BillingAddress: new AddressDto("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001"),
        Payment: new PaymentDto("John Doe", "4111111111111111", "12/26", "123", 1),
        Status: "Pending",
        OrderItems: [new OrderItemDto(Guid.NewGuid(), Guid.NewGuid(), 1, 499.99m)],
        TotalPrice: 499.99m,
        CreatedAt: DateTime.UtcNow);

    [Fact]
    public async Task CreateOrder_ThenGetOrders_ShouldReturnOrder()
    {
        var handler = new CreateOrderHandler(_repository);
        var dto = BuildOrderDto("ORD_INT_001");

        var result = await handler.Handle(new CreateOrderCommand(dto), CancellationToken.None);

        result.Id.Should().NotBeEmpty();

        var getHandler = new GetOrdersHandler(_repository);
        var orders = await getHandler.Handle(new GetOrdersQuery(), CancellationToken.None);

        orders.Orders.Should().Contain(o => o.OrderName == "ORD_INT_001");
    }

    [Fact]
    public async Task CreateOrder_ThenGetByCustomer_ShouldReturnOrder()
    {
        var customerId = Guid.NewGuid();
        var dto = new OrderDto(Guid.NewGuid(), customerId, "ORD_CUST_001",
            new AddressDto("Jane", "Doe", "jane@example.com", "456 Elm St", "USA", "CA", "90001"),
            new AddressDto("Jane", "Doe", "jane@example.com", "456 Elm St", "USA", "CA", "90001"),
            new PaymentDto("Jane Doe", "4111111111111111", "12/26", "321", 1),
            "Pending",
            [new OrderItemDto(Guid.NewGuid(), Guid.NewGuid(), 1, 299.99m)],
            299.99m,
            DateTime.UtcNow);

        var createHandler = new CreateOrderHandler(_repository);
        await createHandler.Handle(new CreateOrderCommand(dto), CancellationToken.None);

        var getHandler = new GetOrdersByCustomerHandler(_repository);
        var result = await getHandler.Handle(new GetOrdersByCustomerQuery(customerId), CancellationToken.None);

        result.Orders.Should().Contain(o => o.CustomerId == customerId);
    }

    [Fact]
    public async Task CreateOrder_ThenUpdateStatus_ShouldBeProcessing()
    {
        var createHandler = new CreateOrderHandler(_repository);
        var dto = BuildOrderDto("ORD_UPD_001");
        var created = await createHandler.Handle(new CreateOrderCommand(dto), CancellationToken.None);

        var updateDto = dto with { Id = created.Id, Status = "Processing" };
        var updateHandler = new UpdateOrderHandler(_repository);
        var updateResult = await updateHandler.Handle(new UpdateOrderCommand(updateDto), CancellationToken.None);

        updateResult.IsSuccess.Should().BeTrue();

        var order = await _repository.GetOrderByIdAsync(created.Id);
        order!.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public async Task CreateOrder_ThenDelete_ShouldNotExist()
    {
        var createHandler = new CreateOrderHandler(_repository);
        var dto = BuildOrderDto("ORD_DEL_001");
        var created = await createHandler.Handle(new CreateOrderCommand(dto), CancellationToken.None);

        var deleteHandler = new DeleteOrderHandler(_repository);
        var deleteResult = await deleteHandler.Handle(new DeleteOrderCommand(created.Id), CancellationToken.None);

        deleteResult.IsSuccess.Should().BeTrue();

        var order = await _repository.GetOrderByIdAsync(created.Id);
        order.Should().BeNull();
    }
}
