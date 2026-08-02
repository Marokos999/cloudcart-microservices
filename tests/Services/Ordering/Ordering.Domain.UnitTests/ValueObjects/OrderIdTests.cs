using FluentAssertions;
using Ordering.Domain.Abstractions;
using Ordering.Domain.ValueObjects;

namespace Ordering.Domain.UnitTests.ValueObjects;

public class OrderIdTests
{
    [Fact]
    public void Of_WithValidGuid_ReturnsOrderId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var orderId = OrderId.Of(guid);

        // Assert
        orderId.Value.Should().Be(guid);
    }

    [Fact]
    public void Of_WithEmptyGuid_ThrowsDomainException()
    {
        // Arrange & Act
        var act = () => OrderId.Of(Guid.Empty);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*OrderId cannot be empty*");
    }

    [Fact]
    public void TwoOrderIds_WithSameGuid_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var a = OrderId.Of(guid);
        var b = OrderId.Of(guid);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TwoOrderIds_WithDifferentGuids_AreNotEqual()
    {
        // Arrange & Act
        var a = OrderId.Of(Guid.NewGuid());
        var b = OrderId.Of(Guid.NewGuid());

        // Assert
        a.Should().NotBe(b);
    }
}
