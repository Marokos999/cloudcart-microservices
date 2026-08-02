using CloudCart.BuildingBlocks.Pagination;
using MediatR;

namespace Catalog.API.Features.GetProducts;

public static class GetProductEndpoints
{
    public static IEndpointRouteBuilder MapGetProductsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async ([AsParameters] PaginationRequest request, ISender sender) =>
        {
            var result = await sender.Send(new GetProductsQuery(request));
            return Results.Ok(result.Products);
        })
        .WithName("GetProducts")
        .WithSummary("Get paginated list of products")
        .WithTags("Catalog");

        return app;
    }
}