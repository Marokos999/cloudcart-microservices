using MediatR;

namespace Basket.API.Features.CheckoutBasket;

public static class CheckoutBasketEndpoint
{
    public static IEndpointRouteBuilder MapCheckoutBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/checkout", async (CheckoutBasketRequest request, ISender sender) =>
        {
            var command = new CheckoutBasketCommand(request);
            var result = await sender.Send(command);
            return Results.Accepted();
        });

        return app;
    }
}