using FluentAssertions;
using Ordering.Domain.ValueObjects;

namespace Ordering.Domain.UnitTests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Of_WithValidFields_ReturnsAddress()
    {
        // Arrange & Act
        var address = Address.Of("John", "Doe", "john@example.com", "123 Main St", "US", "NY", "10001");

        // Assert
        address.FirstName.Should().Be("John");
        address.LastName.Should().Be("Doe");
        address.EmailAddress.Should().Be("john@example.com");
        address.AddressLine.Should().Be("123 Main St");
        address.Country.Should().Be("US");
        address.State.Should().Be("NY");
        address.ZipCode.Should().Be("10001");
    }

    [Fact]
    public void Of_WithNullFirstName_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Address.Of(null!, "Doe", "john@example.com", "123 Main St", "US", "NY", "10001");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Of_WithNullLastName_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Address.Of("John", null!, "john@example.com", "123 Main St", "US", "NY", "10001");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Of_WithNullEmailAddress_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Address.Of("John", "Doe", null!, "123 Main St", "US", "NY", "10001");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Of_WithNullAddressLine_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Address.Of("John", "Doe", "john@example.com", null!, "US", "NY", "10001");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TwoAddresses_WithSameValues_AreEqual()
    {
        // Arrange & Act
        var a = Address.Of("Jane", "Smith", "jane@example.com", "5 Broadway", "US", "CA", "90001");
        var b = Address.Of("Jane", "Smith", "jane@example.com", "5 Broadway", "US", "CA", "90001");

        // Assert
        a.Should().Be(b);
    }
}
