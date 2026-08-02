using Discount.Grpc.Data;
using Discount.Grpc.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.UnitTests.Services;

public class CreateDiscountHandlerTests
{
    private static DiscountContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DiscountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DiscountContext(options);
    }

    [Fact]
    public async Task CreateDiscount_ShouldPersistCoupon_AndReturnCorrectValues()
    {
        // Arrange
        var context = CreateContext();
        var service = new DiscountService(context);
        var request = new CreateDiscountRequest
        {
            ProductName = "Xbox Series X",
            Description = "Xbox Discount",
            Amount = 150
        };

        // Act
        var result = await service.CreateDiscount(request, null!);

        // Assert
        result.ProductName.Should().Be("Xbox Series X");
        result.Description.Should().Be("Xbox Discount");
        result.Amount.Should().Be(150);
        result.Id.Should().BeGreaterThan(0);

        var saved = await context.Coupons.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.ProductName.Should().Be("Xbox Series X");
    }

    [Fact]
    public async Task CreateDiscount_WithZeroAmount_ShouldPersist()
    {
        // Arrange
        var context = CreateContext();
        var service = new DiscountService(context);
        var request = new CreateDiscountRequest
        {
            ProductName = "Free Item",
            Description = "No cost",
            Amount = 0
        };

        // Act
        var result = await service.CreateDiscount(request, null!);

        // Assert
        result.Amount.Should().Be(0);
        result.ProductName.Should().Be("Free Item");
    }
}
