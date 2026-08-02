using MediatR;

namespace Catalog.API.Features.GetProductById;

public static class GetProductByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id));
            return Results.Ok(result.Product);
        })
        .WithName("GetProductId")
        .WithSummary("Get product by id")
        .WithTags("Catalog");

        return app;
    }
}