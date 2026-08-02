using Catalog.API.Data;
using Catalog.API.Features.UpdateProduct;
using Catalog.API.Models;
using CloudCart.BuildingBlocks.Exceptions;
using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Catalog.API.UnitTests.Features;

public class UpdateProductHandlerTests
{
    private readonly ICatalogContext _context;
    private readonly IMongoCollection<Product> _collection;
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _context = Substitute.For<ICatalogContext>();
        _collection = Substitute.For<IMongoCollection<Product>>();
        _context.Products.Returns(_collection);
        _handler = new UpdateProductHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProduct_WhenExists()
    {
        var productId = Guid.NewGuid().ToString();
        var existing = new Product
        {
            Id = productId,
            Name = "Old Name",
            Category = "Consoles",
            Description = "Old desc",
            ImageFile = "old.jpg",
            Price = 299.99m,
        };

        var cursor = Substitute.For<IAsyncCursor<Product>>();
        cursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        cursor.Current.Returns([existing]);

        _collection
            .FindAsync(Arg.Any<FilterDefinition<Product>>(), Arg.Any<FindOptions<Product, Product>>(), Arg.Any<CancellationToken>())
            .Returns(cursor);

        var command = new UpdateProductCommand(productId, "New Name", "Consoles", "New desc", "new.jpg", 399.99m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _collection.Received(1).ReplaceOneAsync(
            Arg.Any<FilterDefinition<Product>>(),
            Arg.Is<Product>(p => p.Name == "New Name" && p.Price == 399.99m),
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        var cursor = Substitute.For<IAsyncCursor<Product>>();
        cursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(false);
        cursor.Current.Returns([]);

        _collection
            .FindAsync(Arg.Any<FilterDefinition<Product>>(), Arg.Any<FindOptions<Product, Product>>(), Arg.Any<CancellationToken>())
            .Returns(cursor);

        var command = new UpdateProductCommand(Guid.NewGuid().ToString(), "Name", "Cat", "Desc", "img.jpg", 100m);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
