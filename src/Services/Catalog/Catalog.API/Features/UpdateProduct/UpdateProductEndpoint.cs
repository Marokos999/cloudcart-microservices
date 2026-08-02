using MediatR;

namespace Catalog.API.Features.UpdateProduct;

public record UpdateProductRequest(
    string Id, 
    string Name,
    string Category,
    string Description,
    string ImageFile,
    decimal Price
);

public static class UpdateProductEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProductEndpoint(this IEndpointRouteBuilder app)
    {

        app.MapPut("/products/{id}", async (string id, UpdateProductRequest request,ISender sender) =>
        {
            var result = await sender.Send(new UpdateProductCommand(
                id,
                request.Name,
                request.Category,
                request.Description,
                request.ImageFile,
                request.Price));

                return Results.Ok(result);
        })
        .WithName("UpdateProduct")
        .WithSummary("Update existing product")
        .WithTags("Catalog");

        return app;
    }
}