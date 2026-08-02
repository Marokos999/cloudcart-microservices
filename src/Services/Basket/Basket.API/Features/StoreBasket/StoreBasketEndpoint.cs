using Basket.API.Models;
using MediatR;

namespace Basket.API.Features.StoreBasket;

public static class StoreBasketEndpoint
{
    public static IEndpointRouteBuilder MapStoreBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/basket", async (ShoppingCart cart, ISender sender) =>
        {
            var result = await sender.Send(new StoreBasketCommand(cart));
            return Results.Created($"/basket/{result.UserName}", result);
        })
        .WithName("StoreBasket")
        .WithSummary("Store shopping cart")
        .WithTags("Basket");

        return app;
    }
}
