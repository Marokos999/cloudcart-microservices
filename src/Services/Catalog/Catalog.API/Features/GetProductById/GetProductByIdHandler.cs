using Catalog.API.Data;
using Catalog.API.Models;
using CloudCart.BuildingBlocks.CQRS;
using CloudCart.BuildingBlocks.Exceptions;
using MongoDB.Driver;

namespace Catalog.API.Features.GetProductById;

public record GetProductByIdQuery(string Id) : IQuery<GetProductByIdRequest>;
public record GetProductByIdRequest(Product Product);

public class GetProductByIdHandler(ICatalogContext context) : IQueryHandler<GetProductByIdQuery, GetProductByIdRequest>
{
    public async Task<GetProductByIdRequest> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await context.Products
                      .Find(p => p.Id == query.Id)
                      .FirstOrDefaultAsync(cancellationToken);
        
        if(product is null)
        throw new NotFoundException($"Product {query.Id}");

        return new GetProductByIdRequest(product);
    }
}

