#pragma warning disable CS8619
using CloudCart.BuildingBlocks.Exceptions;
using FluentAssertions;
using Inventory.Application.Contracts;
using Inventory.Application.Features.InventoryItems.Commands.UpdateInventoryItem;
using Inventory.Domain.Models;
using Inventory.Domain.ValueObjects;
using NSubstitute;

namespace Inventory.API.UnitTests.Features;

public class UpdateInventoryItemHandlerTests
{
    private readonly IInventoryRepository _repository;
    private readonly UpdateInventoryItemHandler _handler;

    public UpdateInventoryItemHandlerTests()
    {
        _repository = Substitute.For<IInventoryRepository>();
        _handler = new UpdateInventoryItemHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldAddStock_WhenNewQuantityIsHigher()
    {
        var item = InventoryItem.Create(InventoryItemId.Of(Guid.NewGuid()), ProductId.Of(Guid.NewGuid()), "Mouse", 10);
        _repository.GetByIdAsync(item.Id.Value, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _handler.Handle(new UpdateInventoryItemCommand(item.Id.Value, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        item.Quantity.Should().Be(20);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRemoveStock_WhenNewQuantityIsLower()
    {
        var item = InventoryItem.Create(InventoryItemId.Of(Guid.NewGuid()), ProductId.Of(Guid.NewGuid()), "Keyboard", 30);
        _repository.GetByIdAsync(item.Id.Value, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _handler.Handle(new UpdateInventoryItemCommand(item.Id.Value, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        item.Quantity.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WhenItemNotFound_ShouldThrow()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((InventoryItem?)null);

        var act = async () => await _handler.Handle(new UpdateInventoryItemCommand(Guid.NewGuid(), 5), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
