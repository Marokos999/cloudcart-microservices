using Basket.API.Data;
using Basket.API.Features.DeleteBasket;
using FluentAssertions;
using NSubstitute;

namespace Basket.API.UnitTests.Features;

public class DeleteBasketHandlerTests
{
    private readonly IBasketRepository _repository;
    private readonly DeleteBasketHandler _handler;

    public DeleteBasketHandlerTests()
    {
        _repository = Substitute.For<IBasketRepository>();
        _handler = new DeleteBasketHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenBasketDeleted()
    {
        _repository.DeleteBasket("john_doe", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(new DeleteBasketCommand("john_doe"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenBasketNotFound()
    {
        _repository.DeleteBasket("unknown", Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(new DeleteBasketCommand("unknown"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
