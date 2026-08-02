#pragma warning disable CS8619
using CloudCart.BuildingBlocks.Exceptions;
using FluentAssertions;
using Inventory.Application.Contracts;
using Inventory.Application.Features.InventoryItems.Commands.DeleteInventoryItem;
using Inventory.Domain.Models;
using Inventory.Domain.ValueObjects;
using NSubstitute;

namespace Inventory.API.UnitTests.Features;

public class DeleteInventoryItemHandlerTests
{
    private readonly IInventoryRepository _repository;
    private readonly DeleteInventoryItemHandler _handler;

    public DeleteInventoryItemHandlerTests()
    {
        _repository = Substitute.For<IInventoryRepository>();
        _handler = new DeleteInventoryItemHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldDeleteItem_AndReturnSuccess()
    {
        var item = InventoryItem.Create(InventoryItemId.Of(Guid.NewGuid()), ProductId.Of(Guid.NewGuid()), "Monitor", 5);
        _repository.GetByIdAsync(item.Id.Value, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _handler.Handle(new DeleteInventoryItemCommand(item.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repository.Received(1).Delete(item);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenItemNotFound_ShouldThrow()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((InventoryItem?)null);

        var act = async () => await _handler.Handle(new DeleteInventoryItemCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
