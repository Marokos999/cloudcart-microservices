using Catalog.API.Data;
using Catalog.API.Models;
using CloudCart.BuildingBlocks.CQRS;
using CloudCart.BuildingBlocks.Pagination;
using MediatR;
using MongoDB.Driver;

namespace Catalog.API.Features.GetProducts;

public record GetProductsQuery(PaginationRequest Pagination) : IQuery<GetProductsResult>;
public record GetProductsResult(PaginatedResult<Product> Products);

public class GetProductsHandler(ICatalogContext context) : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var page = query.Pagination.Page;
        var pageSize = query.Pagination.PageSize;

        var total = await context.Products.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken);

        var products = await context.Products
            .Find(_ => true)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetProductsResult(new PaginatedResult<Product>(products, total, page, pageSize));
    }
}