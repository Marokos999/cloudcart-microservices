using MediatR;
using Ordering.Application.Dtos;
using Ordering.Application.Features.Orders.Commands.CreateOrder;
using Ordering.Application.Features.Orders.Commands.DeleteOrder;
using Ordering.Application.Features.Orders.Commands.UpdateOrder;
using Ordering.Application.Features.Orders.Queries.GetOrderById;
using Ordering.Application.Features.Orders.Queries.GetOrders;
using Ordering.Application.Features.Orders.Queries.GetOrdersByCustomer;

namespace Ordering.API.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async (ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersQuery());
            return Results.Ok(result.Orders);
        }).WithName("GetOrders").WithTags("Orders");

        app.MapGet("/orders/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetOrderByIdQuery(id));
            return Results.Ok(result.Order);
        }).WithName("GetOrderById").WithTags("Orders");

        app.MapGet("/orders/customer/{customerId:guid}", async (Guid customerId, ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersByCustomerQuery(customerId));
            return Results.Ok(result.Orders);
        }).WithName("GetOrdersByCustomer").WithTags("Orders");

        app.MapPost("/orders", async (OrderDto order, ISender sender) =>
        {
            var result = await sender.Send(new CreateOrderCommand(order));
            return Results.Created($"/orders/{result.Id}", result);
        }).WithName("CreateOrder").WithTags("Orders");

        app.MapPut("/orders/{id:guid}", async (Guid id, OrderDto order, ISender sender) =>
        {
            var result = await sender.Send(new UpdateOrderCommand(order with { Id = id }));
            return Results.Ok(result);
        }).WithName("UpdateOrder").WithTags("Orders");

        app.MapDelete("/orders/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteOrderCommand(id));
            return Results.Ok(result);
        }).WithName("DeleteOrder").WithTags("Orders");

        return app;
    }
}
