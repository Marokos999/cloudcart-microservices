using FluentAssertions;
using Ordering.Domain.ValueObjects;

namespace Ordering.Domain.UnitTests.ValueObjects;

public class PaymentTests
{
    [Fact]
    public void Of_WithValidFields_ReturnsPayment()
    {
        // Arrange & Act
        var payment = Payment.Of("John Doe", "4111111111111111", "12/26", "123", 1);

        // Assert
        payment.CardName.Should().Be("John Doe");
        payment.CardNumber.Should().Be("4111111111111111");
        payment.Expiration.Should().Be("12/26");
        payment.Cvv.Should().Be("123");
        payment.PaymentMethod.Should().Be(1);
    }

    [Fact]
    public void Of_WithNullCardName_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Payment.Of(null!, "4111111111111111", "12/26", "123", 1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Of_WithNullCardNumber_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Payment.Of("John Doe", null!, "12/26", "123", 1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Of_WithNullExpiration_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Payment.Of("John Doe", "4111111111111111", null!, "123", 1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Of_WithNullCvv_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Payment.Of("John Doe", "4111111111111111", "12/26", null!, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TwoPayments_WithSameValues_AreEqual()
    {
        // Arrange & Act
        var a = Payment.Of("Jane Smith", "4242424242424242", "01/27", "456", 2);
        var b = Payment.Of("Jane Smith", "4242424242424242", "01/27", "456", 2);

        // Assert
        a.Should().Be(b);
    }
}
