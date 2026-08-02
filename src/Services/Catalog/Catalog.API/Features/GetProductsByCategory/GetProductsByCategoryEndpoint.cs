using CloudCart.BuildingBlocks.Pagination;
using MediatR;

namespace Catalog.API.Features.GetProductsByCategory;

public static class GetProductsByCategoryEndpoint
{
    public static IEndpointRouteBuilder MapGetProductsByCategoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products/category/{category}", async (string category, [AsParameters] PaginationRequest request, ISender sender) =>
        {
            var result = await sender.Send(new GetProductsByCategoryQuery(category, request));
            return Results.Ok(result.Products);
        })
        .WithName("GetProductsByCategory")
        .WithSummary("Get paginated products filtered by category")
        .WithTags("Catalog");

        return app;
    }
}
