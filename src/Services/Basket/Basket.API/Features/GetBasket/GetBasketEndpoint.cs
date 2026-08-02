using MediatR;

namespace Basket.API.Features.GetBasket;

public static class GetBasketEndpoint
{
    public static IEndpointRouteBuilder MapGetBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/basket/{userName}", async (string userName, ISender sender) =>
        {
            var result = await sender.Send(new GetBasketQuery(userName));
            return Results.Ok(result.Cart);
        })
        .WithName("GetBasket")
        .WithSummary("Get shopping cart by username")
        .WithTags("Basket");

        return app;
    }
}