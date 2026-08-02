using FluentAssertions;
using Inventory.Application.Features.InventoryItems.Commands.CreateInventoryItem;
using Inventory.Application.Features.InventoryItems.Commands.DeleteInventoryItem;
using Inventory.Application.Features.InventoryItems.Commands.UpdateInventoryItem;
using Inventory.Application.Features.InventoryItems.Queries.GetInventoryItemById;
using Inventory.Application.Features.InventoryItems.Queries.GetInventoryItems;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Inventory.API.IntegrationTests.Features;

public class InventoryIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private InventoryContext _context = null!;
    private InventoryRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<InventoryContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .Options;

        _context = new InventoryContext(options);
        await _context.Database.MigrateAsync();
        _repository = new InventoryRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _sqlContainer.StopAsync();
    }

    [Fact]
    public async Task CreateItem_ThenGetAll_ShouldReturnItem()
    {
        var createHandler = new CreateInventoryItemHandler(_repository);
        var command = new CreateInventoryItemCommand(Guid.NewGuid().ToString(), "Gaming Mouse", 50);

        var created = await createHandler.Handle(command, CancellationToken.None);

        created.Id.Should().NotBeEmpty();

        var getHandler = new GetInventoryItemsHandler(_repository);
        var result = await getHandler.Handle(new GetInventoryItemsQuery(), CancellationToken.None);

        result.Items.Should().Contain(i => i.ProductName == "Gaming Mouse");
    }

    [Fact]
    public async Task CreateItem_ThenGetById_ShouldReturnCorrectItem()
    {
        var createHandler = new CreateInventoryItemHandler(_repository);
        var productId = Guid.NewGuid().ToString();
        var command = new CreateInventoryItemCommand(productId, "Mechanical Keyboard", 30);

        var created = await createHandler.Handle(command, CancellationToken.None);

        var getHandler = new GetInventoryItemByIdHandler(_repository);
        var result = await getHandler.Handle(new GetInventoryItemByIdQuery(created.Id), CancellationToken.None);

        result.Item.ProductName.Should().Be("Mechanical Keyboard");
        result.Item.Quantity.Should().Be(30);
    }

    [Fact]
    public async Task CreateItem_ThenUpdate_QuantityShouldChange()
    {
        var createHandler = new CreateInventoryItemHandler(_repository);
        var command = new CreateInventoryItemCommand(Guid.NewGuid().ToString(), "Headset", 10);
        var created = await createHandler.Handle(command, CancellationToken.None);

        var updateHandler = new UpdateInventoryItemHandler(_repository);
        var updateResult = await updateHandler.Handle(new UpdateInventoryItemCommand(created.Id, 25), CancellationToken.None);

        updateResult.IsSuccess.Should().BeTrue();

        var getHandler = new GetInventoryItemByIdHandler(_repository);
        var item = await getHandler.Handle(new GetInventoryItemByIdQuery(created.Id), CancellationToken.None);
        item.Item.Quantity.Should().Be(25);
    }

    [Fact]
    public async Task CreateItem_ThenDelete_ShouldNotExist()
    {
        var createHandler = new CreateInventoryItemHandler(_repository);
        var command = new CreateInventoryItemCommand(Guid.NewGuid().ToString(), "Monitor", 5);
        var created = await createHandler.Handle(command, CancellationToken.None);

        var deleteHandler = new DeleteInventoryItemHandler(_repository);
        var deleteResult = await deleteHandler.Handle(new DeleteInventoryItemCommand(created.Id), CancellationToken.None);

        deleteResult.IsSuccess.Should().BeTrue();

        var item = await _repository.GetByIdAsync(created.Id);
        item.Should().BeNull();
    }
}
