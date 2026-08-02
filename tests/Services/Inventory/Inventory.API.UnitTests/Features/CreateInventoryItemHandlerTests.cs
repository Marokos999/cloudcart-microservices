#pragma warning disable CS8619
using FluentAssertions;
using Inventory.Application.Contracts;
using Inventory.Application.Features.InventoryItems.Commands.CreateInventoryItem;
using Inventory.Domain.Models;
using NSubstitute;

namespace Inventory.API.UnitTests.Features;

public class CreateInventoryItemHandlerTests
{
    private readonly IInventoryRepository _repository;
    private readonly CreateInventoryItemHandler _handler;

    public CreateInventoryItemHandlerTests()
    {
        _repository = Substitute.For<IInventoryRepository>();
        _handler = new CreateInventoryItemHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldCreateInventoryItem_AndReturnId()
    {
        _repository.AddAsync(Arg.Any<InventoryItem>(), Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult(x.Arg<InventoryItem>()));

        var command = new CreateInventoryItemCommand(Guid.NewGuid().ToString(), "Gaming Mouse", 100);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsync_Once()
    {
        _repository.AddAsync(Arg.Any<InventoryItem>(), Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult(x.Arg<InventoryItem>()));

        var command = new CreateInventoryItemCommand(Guid.NewGuid().ToString(), "Keyboard", 50);
        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(Arg.Any<InventoryItem>(), Arg.Any<CancellationToken>());
    }
}
