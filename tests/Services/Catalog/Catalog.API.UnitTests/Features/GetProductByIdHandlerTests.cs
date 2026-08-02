using Catalog.API.Data;
using Catalog.API.Features.GetProductById;
using Catalog.API.Models;
using CloudCart.BuildingBlocks.Exceptions;
using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Catalog.API.UnitTests.Features;

public class GetProductByIdHandlerTests
{
    private readonly ICatalogContext _context;
    private readonly IMongoCollection<Product> _collection;
    private readonly GetProductByIdHandler _handler;

    public GetProductByIdHandlerTests()
    {
        _context = Substitute.For<ICatalogContext>();
        _collection = Substitute.For<IMongoCollection<Product>>();
        _context.Products.Returns(_collection);
        _handler = new GetProductByIdHandler(_context);
    }

    [Fact]
    public void GetProductByIdQuery_ShouldContainId()
    {
        var query = new GetProductByIdQuery("abc123");

        query.Id.Should().Be("abc123");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        var cursor = Substitute.For<IAsyncCursor<Product>>();
        cursor.MoveNext(Arg.Any<CancellationToken>()).Returns(false);
        cursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(false);
        _collection.FindAsync(Arg.Any<FilterDefinition<Product>>(),
            Arg.Any<FindOptions<Product, Product>>(),
            Arg.Any<CancellationToken>()).Returns(cursor);

        var query = new GetProductByIdQuery("nonexistent-id");

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
