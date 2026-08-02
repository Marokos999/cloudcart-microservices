using FluentAssertions;
using Ordering.Domain.Abstractions;
using Ordering.Domain.ValueObjects;

namespace Ordering.Domain.UnitTests.ValueObjects;

public class OrderNameTests
{
    [Fact]
    public void Of_WithValidValue_ReturnsOrderName()
    {
        // Arrange & Act
        var orderName = OrderName.Of("ORD-2024-001");

        // Assert
        orderName.Value.Should().Be("ORD-2024-001");
    }

    [Fact]
    public void Of_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => OrderName.Of(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Of_WithEmptyString_ThrowsDomainException()
    {
        // Arrange & Act
        var act = () => OrderName.Of("");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*OrderName cannot be empty*");
    }

    [Fact]
    public void Of_WithWhitespaceOnly_ThrowsDomainException()
    {
        // Arrange & Act
        var act = () => OrderName.Of("   ");

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TwoOrderNames_WithSameValue_AreEqual()
    {
        // Arrange & Act
        var a = OrderName.Of("ORD-001");
        var b = OrderName.Of("ORD-001");

        // Assert
        a.Should().Be(b);
    }
}
