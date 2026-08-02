using Catalog.API.Data;
using Catalog.API.Features.CreateProduct;
using Catalog.API.Models;
using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;

namespace Catalog.API.UnitTests.Features;

public class CreateProductHandlerTests
{
    private readonly ICatalogContext _context;
    private readonly IMongoCollection<Product> _collection;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _context = Substitute.For<ICatalogContext>();
        _collection = Substitute.For<IMongoCollection<Product>>();
        _context.Products.Returns(_collection);
        _handler = new CreateProductHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldInsertProductAndReturnId()
    {
        var command = new CreateProductCommand(
            "PlayStation 5",
            "Consoles",
            "Next-gen console",
            "ps5.jpg",
            499.99m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        await _collection.Received(1).InsertOneAsync(
            Arg.Is<Product>(p =>
                p.Name == "PlayStation 5" &&
                p.Category == "Consoles" &&
                p.Price == 499.99m),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CreateProductCommand_ShouldHaveCorrectProperties()
    {
        var command = new CreateProductCommand(
            "Xbox Series X",
            "Consoles",
            "Microsoft console",
            "xbox.jpg",
            449.99m);

        command.Name.Should().Be("Xbox Series X");
        command.Category.Should().Be("Consoles");
        command.Price.Should().Be(449.99m);
    }
}
