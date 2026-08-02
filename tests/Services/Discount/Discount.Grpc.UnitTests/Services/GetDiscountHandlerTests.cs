using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.UnitTests.Services;

public class GetDiscountHandlerTests
{
    private static DiscountContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DiscountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DiscountContext(options);
    }

    [Fact]
    public async Task GetDiscount_WhenCouponExists_ReturnsCoupon()
    {
        // Arrange
        var context = CreateContext();
        context.Coupons.Add(new Coupon { Id = 10, ProductName = "PlayStation 5", Description = "PS5 Discount", Amount = 200 });
        await context.SaveChangesAsync();

        var service = new DiscountService(context);
        var request = new GetDiscountRequest { ProductName = "PlayStation 5" };

        // Act
        var result = await service.GetDiscount(request, null!);

        // Assert
        result.ProductName.Should().Be("PlayStation 5");
        result.Amount.Should().Be(200);
        result.Description.Should().Be("PS5 Discount");
        result.Id.Should().Be(10);
    }

    [Fact]
    public async Task GetDiscount_WhenCouponNotFound_ReturnsNoDiscount()
    {
        // Arrange
        var context = CreateContext();
        var service = new DiscountService(context);
        var request = new GetDiscountRequest { ProductName = "NonExistentProduct" };

        // Act
        var result = await service.GetDiscount(request, null!);

        // Assert
        result.ProductName.Should().Be("No Discount");
        result.Amount.Should().Be(0);
        result.Description.Should().Be("No Discount");
    }
}
