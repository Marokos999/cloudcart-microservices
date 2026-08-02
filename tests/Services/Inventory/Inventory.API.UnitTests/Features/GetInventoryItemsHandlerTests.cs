#pragma warning disable CS8619
using FluentAssertions;
using Inventory.Application.Contracts;
using Inventory.Application.Features.InventoryItems.Queries.GetInventoryItems;
using Inventory.Domain.Models;
using Inventory.Domain.ValueObjects;
using NSubstitute;

namespace Inventory.API.UnitTests.Features;

public class GetInventoryItemsHandlerTests
{
    private readonly IInventoryRepository _repository;
    private readonly GetInventoryItemsHandler _handler;

    public GetInventoryItemsHandlerTests()
    {
        _repository = Substitute.For<IInventoryRepository>();
        _handler = new GetInventoryItemsHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllItems()
    {
        var items = new List<InventoryItem>
        {
            InventoryItem.Create(InventoryItemId.Of(Guid.NewGuid()), ProductId.Of(Guid.NewGuid()), "Mouse", 10),
            InventoryItem.Create(InventoryItemId.Of(Guid.NewGuid()), ProductId.Of(Guid.NewGuid()), "Keyboard", 5)
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(items);

        var result = await _handler.Handle(new GetInventoryItemsQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoItems_ShouldReturnEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await _handler.Handle(new GetInventoryItemsQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
    }
}
