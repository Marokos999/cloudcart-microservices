using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Services;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.UnitTests.Services;

public class UpdateDiscountHandlerTests
{
    private static DiscountContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DiscountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DiscountContext(options);
    }

    [Fact]
    public async Task UpdateDiscount_WhenCouponExists_UpdatesAndReturnsUpdatedValues()
    {
        // Arrange
        var context = CreateContext();
        context.Coupons.Add(new Coupon { Id = 1, ProductName = "Old Name", Description = "Old Desc", Amount = 50 });
        await context.SaveChangesAsync();

        var service = new DiscountService(context);
        var request = new UpdateDiscountRequest
        {
            Id = 1,
            ProductName = "New Name",
            Description = "New Desc",
            Amount = 100
        };

        // Act
        var result = await service.UpdateDiscount(request, null!);

        // Assert
        result.Id.Should().Be(1);
        result.ProductName.Should().Be("New Name");
        result.Description.Should().Be("New Desc");
        result.Amount.Should().Be(100);
    }

    [Fact]
    public async Task UpdateDiscount_WhenCouponNotFound_ThrowsRpcException()
    {
        // Arrange
        var context = CreateContext();
        var service = new DiscountService(context);
        var request = new UpdateDiscountRequest { Id = 999, ProductName = "X", Description = "X", Amount = 0 };

        // Act
        var act = async () => await service.UpdateDiscount(request, null!);

        // Assert
        await act.Should().ThrowAsync<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound);
    }
}
