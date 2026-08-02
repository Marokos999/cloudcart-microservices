using MediatR;

namespace Catalog.API.Features.CreateProduct;

public record CreateProductRequest(
    string Name,
    string Category,
    string Description,
    string ImageFile,
    decimal Price);

public static class CreateProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateProductCommand(
                request.Name,
                request.Category,
                request.Description,
                request.ImageFile,
                request.Price));

            return Results.Created($"/products/{result.Id}", result);
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product")
        .WithTags("Catalog");

        return app;
    }
}
