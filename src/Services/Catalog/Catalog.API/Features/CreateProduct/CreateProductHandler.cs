using Catalog.API.Data;
using Catalog.API.Models;
using CloudCart.BuildingBlocks.CQRS;

namespace Catalog.API.Features.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Category,
    string Description,
    string ImageFile,
    decimal Price
) : ICommand<CreateProductResult>;

public record CreateProductResult(string Id);

public class CreateProductHandler(ICatalogContext context) : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = command.Name,
            Category = command.Category,
            Description = command.Description,
            ImageFile = command.ImageFile,
            Price = command.Price
        };

        await context.Products.InsertOneAsync(product, cancellationToken:cancellationToken);

        return new CreateProductResult(product.Id);
    }
}