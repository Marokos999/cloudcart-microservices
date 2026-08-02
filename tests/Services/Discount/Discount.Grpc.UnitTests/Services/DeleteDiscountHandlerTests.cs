using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Services;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.UnitTests.Services;

public class DeleteDiscountHandlerTests
{
    private static DiscountContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DiscountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DiscountContext(options);
    }

    [Fact]
    public async Task DeleteDiscount_WhenCouponExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        var context = CreateContext();
        context.Coupons.Add(new Coupon { Id = 5, ProductName = "Nintendo Switch", Description = "Switch Discount", Amount = 50 });
        await context.SaveChangesAsync();

        var service = new DiscountService(context);
        var request = new DeleteDiscountRequest { Id = 5 };

        // Act
        var result = await service.DeleteDiscount(request, null!);

        // Assert
        result.Success.Should().BeTrue();
        var deleted = await context.Coupons.FindAsync(5);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteDiscount_WhenCouponNotFound_ThrowsRpcException()
    {
        // Arrange
        var context = CreateContext();
        var service = new DiscountService(context);
        var request = new DeleteDiscountRequest { Id = 999 };

        // Act
        var act = async () => await service.DeleteDiscount(request, null!);

        // Assert
        await act.Should().ThrowAsync<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound);
    }
}
