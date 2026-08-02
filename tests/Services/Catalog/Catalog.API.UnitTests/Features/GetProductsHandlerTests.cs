using Catalog.API.Data;
using Catalog.API.Features.GetProducts;
using Catalog.API.Models;
using CloudCart.BuildingBlocks.Pagination;
using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;

namespace Catalog.API.UnitTests.Features;

public class GetProductsHandlerTests
{
    private readonly ICatalogContext _context;
    private readonly IMongoCollection<Product> _collection;
    private readonly GetProductsHandler _handler;

    public GetProductsHandlerTests()
    {
        _context = Substitute.For<ICatalogContext>();
        _collection = Substitute.For<IMongoCollection<Product>>();
        _context.Products.Returns(_collection);
        _handler = new GetProductsHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = "1", Name = "PlayStation 5", Category = "Consoles", Price = 499.99m },
            new() { Id = "2", Name = "Xbox Series X", Category = "Consoles", Price = 449.99m }
        };

        _collection.CountDocumentsAsync(default!, default, default).ReturnsForAnyArgs(Task.FromResult(2L));

        var cursor = Substitute.For<IAsyncCursor<Product>>();
        cursor.Current.Returns(products);
        cursor.MoveNextAsync(default).ReturnsForAnyArgs(Task.FromResult(true), Task.FromResult(false));

        // Find() is an extension method that internally calls FindAsync on the interface
        _collection.FindAsync(default(FilterDefinition<Product>)!, default(FindOptions<Product, Product>), default)
            .ReturnsForAnyArgs(Task.FromResult<IAsyncCursor<Product>>(cursor));

        var query = new GetProductsQuery(new PaginationRequest(1, 10));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Products.Should().NotBeNull();
        result.Products.Data.Should().HaveCount(2);
        result.Products.Total.Should().Be(2);
        result.Products.Page.Should().Be(1);
        result.Products.PageSize.Should().Be(10);
    }

    [Fact]
    public void GetProductQuery_ShouldHaveCorrectPagination()
    {
        // Arrange && Act
        var query = new GetProductsQuery(new PaginationRequest(2, 5));

        // Assert
        query.Pagination.Page.Should().Be(2);
        query.Pagination.PageSize.Should().Be(5);
    }
}
