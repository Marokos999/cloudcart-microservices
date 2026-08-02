using Catalog.API.Data;
using CloudCart.BuildingBlocks.CQRS;
using CloudCart.BuildingBlocks.Exceptions;
using MongoDB.Driver;

namespace Catalog.API.Features.DeleteProduct;

public record DeleteProductCommand(string Id) : ICommand<DeleteProductResult>;
public record DeleteProductResult(bool IsSuccess);

public class DeleteProductHandler(ICatalogContext context) : ICommandHandler<DeleteProductCommand, DeleteProductResult>
{
    public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var result = await context.Products
            .DeleteOneAsync(p => p.Id == command.Id, cancellationToken);

        if (result.DeletedCount == 0)
            throw new NotFoundException("Product", command.Id);

        return new DeleteProductResult(true);
    }
}
