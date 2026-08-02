using Inventory.Application.Features.InventoryItems.Commands.CreateInventoryItem;
using Inventory.Application.Features.InventoryItems.Commands.DeleteInventoryItem;
using Inventory.Application.Features.InventoryItems.Commands.UpdateInventoryItem;
using Inventory.Application.Features.InventoryItems.Queries.GetInventoryItemById;
using Inventory.Application.Features.InventoryItems.Queries.GetInventoryItemByProductId;
using Inventory.Application.Features.InventoryItems.Queries.GetInventoryItems;
using MediatR;

namespace Inventory.API.Endpoints;

public record UpdateInventoryItemRequest(int Quantity);

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/inventory", async (ISender sender) =>
        {
            var result = await sender.Send(new GetInventoryItemsQuery());
            return Results.Ok(result.Items);
        }).WithName("GetInventoryItems").WithTags("Inventory");

        app.MapGet("/inventory/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetInventoryItemByIdQuery(id));
            return Results.Ok(result.Item);
        }).WithName("GetInventoryItemById").WithTags("Inventory");

        app.MapGet("/inventory/product/{productId:guid}", async (Guid productId, ISender sender) =>
        {
            var result = await sender.Send(new GetInventoryItemByProductIdQuery(productId));
            return Results.Ok(result.Item);
        }).WithName("GetInventoryItemByProductId").WithTags("Inventory");

        app.MapPost("/inventory", async (CreateInventoryItemCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Created($"/inventory/{result.Id}", result);
        }).WithName("CreateInventoryItem").WithTags("Inventory");

        app.MapPut("/inventory/{id:guid}", async (Guid id, UpdateInventoryItemRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateInventoryItemCommand(id, request.Quantity));
            return Results.Ok(result);
        }).WithName("UpdateInventoryItem").WithTags("Inventory");

        app.MapDelete("/inventory/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteInventoryItemCommand(id));
            return Results.Ok(result);
        }).WithName("DeleteInventoryItem").WithTags("Inventory");

        return app;
    }
}
