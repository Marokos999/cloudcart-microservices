using Catalog.API.Data;
using Catalog.API.Features.DeleteProduct;
using Catalog.API.Models;
using CloudCart.BuildingBlocks.Exceptions;
using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;

namespace Catalog.API.UnitTests.Features;

public class DeleteProductHandlerTests
{
    private readonly ICatalogContext _context;
    private readonly IMongoCollection<Product> _collection;
    private readonly DeleteProductHandler _handler;

    public DeleteProductHandlerTests()
    {
        _context = Substitute.For<ICatalogContext>();
        _collection = Substitute.For<IMongoCollection<Product>>();
        _context.Products.Returns(_collection);
        _handler = new DeleteProductHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenProductDeleted()
    {
        var deleteResult = Substitute.For<DeleteResult>();
        deleteResult.DeletedCount.Returns(1);
        _collection.DeleteOneAsync(
            Arg.Any<FilterDefinition<Product>>(),
            Arg.Any<CancellationToken>()).Returns(deleteResult);

        var result = await _handler.Handle(new DeleteProductCommand("existing-id"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProductNotFound()
    {
        var deleteResult = Substitute.For<DeleteResult>();
        deleteResult.DeletedCount.Returns(0);
        _collection.DeleteOneAsync(
            Arg.Any<FilterDefinition<Product>>(),
            Arg.Any<CancellationToken>()).Returns(deleteResult);

        var act = async () => await _handler.Handle(new DeleteProductCommand("nonexistent-id"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
