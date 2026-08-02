using Catalog.API.Data;
using Catalog.API.Models;
using CloudCart.BuildingBlocks.CQRS;
using CloudCart.BuildingBlocks.Pagination;
using MongoDB.Driver;

namespace Catalog.API.Features.GetProductsByCategory;

public record GetProductsByCategoryQuery(string Category, PaginationRequest Pagination) : IQuery<GetProductsByCategoryResult>;
public record GetProductsByCategoryResult(PaginatedResult<Product> Products);

public class GetProductsByCategoryHandler(ICatalogContext context)
    : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryResult>
{
    public async Task<GetProductsByCategoryResult> Handle(GetProductsByCategoryQuery query, CancellationToken cancellationToken)
    {
        var page = query.Pagination.Page;
        var pageSize = query.Pagination.PageSize;
        var filter = Builders<Product>.Filter.Eq(p => p.Category, query.Category);

        var total = await context.Products.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var products = await context.Products
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetProductsByCategoryResult(new PaginatedResult<Product>(products, total, page, pageSize));
    }
}
