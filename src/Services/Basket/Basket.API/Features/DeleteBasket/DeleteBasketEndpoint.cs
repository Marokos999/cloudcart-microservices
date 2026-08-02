using MediatR;

namespace Basket.API.Features.DeleteBasket;

public static class DeleteBasketEndpoint
{
    public static IEndpointRouteBuilder MapDeleteBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket/{userName}", async (string userName, ISender sender) =>
        {
            var result = await sender.Send(new DeleteBasketCommand(userName));
            return Results.Ok(result);
        })
        .WithName("DeleteBasket")
        .WithSummary("Delete shopping cart by username")
        .WithTags("Basket");

        return app;
    }
}