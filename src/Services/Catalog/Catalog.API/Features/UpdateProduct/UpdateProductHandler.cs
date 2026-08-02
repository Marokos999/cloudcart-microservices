using Catalog.API.Data;
using Catalog.API.Models;
using CloudCart.BuildingBlocks.CQRS;
using CloudCart.BuildingBlocks.Exceptions;
using MongoDB.Driver;

namespace Catalog.API.Features.UpdateProduct;

public record UpdateProductCommand(
    string Id, 
    string Name,
    string Category,
    string Description,
    string ImageFile,
    decimal Price
) : ICommand<UpdateProductResult>;

public record UpdateProductResult(bool IsSuccess);

public class UpdateProductHandler(ICatalogContext context) : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
       var product = await context.Products
                     .Find(p => p.Id == command.Id)
                     .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException($"Procut {command.Id}");

        product.Name = command.Name;
        product.Category = command.Category;
        product.Description = command.Description;
        product.ImageFile = command.ImageFile;
        product.Price = command.Price;

        await context.Products.ReplaceOneAsync(p => p.Id == product.Id, product, cancellationToken:cancellationToken);


        return new UpdateProductResult(true);
    }
}
