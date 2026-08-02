using MediatR;

namespace Catalog.API.Features.DeleteProduct;

public static class DeleteProductEndpoint
{
    public static IEndpointRouteBuilder MapDeleteProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id));
            return Results.Ok(result);
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete a product")
        .WithTags("Catalog");

        return app;
    }
}
