using Catalog.API.Data;
using Catalog.API.Features.CreateProduct;
using Catalog.API.Features.DeleteProduct;
using Catalog.API.Features.GetProductById;
using Catalog.API.Features.GetProducts;
using Catalog.API.Features.UpdateProduct;
using Catalog.API.Models;
using CloudCart.BuildingBlocks.Exceptions;
using CloudCart.BuildingBlocks.Pagination;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Catalog.API.IntegrationTests.Features;

public class CatalogIntegrationTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:7.0")
        .Build();

    private ICatalogContext _context = null!;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _mongoContainer.GetConnectionString()
            })
            .Build();

        _context = new CatalogContext(config);

        await CatalogInitialData.SeedAsync(_context.Products);
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.StopAsync();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnSeededProducts()
    {
        var handler = new GetProductsHandler(_context);
        var query = new GetProductsQuery(new PaginationRequest(1, 10));

        var result = await handler.Handle(query, CancellationToken.None);

        result.Products.Data.Should().NotBeEmpty();
        result.Products.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateProduct_ThenGetById_ShouldReturnCreatedProduct()
    {
        var createHandler = new CreateProductHandler(_context);
        var command = new CreateProductCommand(
            "Razer DeathAdder V3",
            "Peripherals",
            "Pro gaming mouse",
            "razer.jpg",
            99.99m);

        var createResult = await createHandler.Handle(command, CancellationToken.None);
        createResult.Id.Should().NotBeNullOrEmpty();

        var getHandler = new GetProductByIdHandler(_context);
        var getResult = await getHandler.Handle(new GetProductByIdQuery(createResult.Id), CancellationToken.None);

        getResult.Product.Name.Should().Be("Razer DeathAdder V3");
        getResult.Product.Price.Should().Be(99.99m);
    }

    [Fact]
    public async Task UpdateProduct_ShouldPersistChanges()
    {
        var createHandler = new CreateProductHandler(_context);
        var createResult = await createHandler.Handle(
            new CreateProductCommand("Old Name", "Consoles", "Desc", "img.jpg", 100m),
            CancellationToken.None);

        var updateHandler = new UpdateProductHandler(_context);
        var updateResult = await updateHandler.Handle(
            new UpdateProductCommand(createResult.Id, "New Name", "Consoles", "New Desc", "img.jpg", 200m),
            CancellationToken.None);

        updateResult.IsSuccess.Should().BeTrue();

        var getHandler = new GetProductByIdHandler(_context);
        var getResult = await getHandler.Handle(new GetProductByIdQuery(createResult.Id), CancellationToken.None);

        getResult.Product.Name.Should().Be("New Name");
        getResult.Product.Price.Should().Be(200m);
    }

    [Fact]
    public async Task DeleteProduct_ShouldRemoveProduct()
    {
        var createHandler = new CreateProductHandler(_context);
        var createResult = await createHandler.Handle(
            new CreateProductCommand("To Delete", "Accessories", "Desc", "img.jpg", 50m),
            CancellationToken.None);

        var deleteHandler = new DeleteProductHandler(_context);
        var deleteResult = await deleteHandler.Handle(
            new DeleteProductCommand(createResult.Id),
            CancellationToken.None);

        deleteResult.IsSuccess.Should().BeTrue();

        var getHandler = new GetProductByIdHandler(_context);
        var act = async () => await getHandler.Handle(new GetProductByIdQuery(createResult.Id), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetProducts_ShouldRespectPagination()
    {
        var handler = new GetProductsHandler(_context);

        var page1 = await handler.Handle(new GetProductsQuery(new PaginationRequest(1, 2)), CancellationToken.None);
        var page2 = await handler.Handle(new GetProductsQuery(new PaginationRequest(2, 2)), CancellationToken.None);

        page1.Products.Data.Should().HaveCount(2);
        page1.Products.Page.Should().Be(1);
        page2.Products.Page.Should().Be(2);
        page1.Products.Data.Select(p => p.Id).Should()
            .NotIntersectWith(page2.Products.Data.Select(p => p.Id));
    }
}
