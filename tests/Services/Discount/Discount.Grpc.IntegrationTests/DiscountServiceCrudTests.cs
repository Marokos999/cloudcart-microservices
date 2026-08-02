using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Services;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.IntegrationTests;

public class DiscountServiceCrudTests
{
    private static DiscountContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DiscountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DiscountContext(options);
    }

    [Fact]
    public async Task Create_ThenGet_ReturnsSameCoupon()
    {
        // Arrange
        var context = CreateContext();
        var service = new DiscountService(context);

        var created = await service.CreateDiscount(new CreateDiscountRequest
        {
            ProductName = "Headphones",
            Description = "Headphones Discount",
            Amount = 30
        }, null!);

        // Act
        var fetched = await service.GetDiscount(new GetDiscountRequest { ProductName = "Headphones" }, null!);

        // Assert
        fetched.ProductName.Should().Be("Headphones");
        fetched.Amount.Should().Be(30);
        fetched.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Create_ThenUpdate_ReturnsUpdatedValues()
    {
        // Arrange
        var context = CreateContext();
        var service = new DiscountService(context);

        var created = await service.CreateDiscount(new CreateDiscountRequest
        {
            ProductName = "Monitor",
            Description = "Monitor Discount",
            Amount = 80
        }, null!);

        // Act
        var updated = await service.UpdateDiscount(new UpdateDiscountRequest
        {
            Id = created.Id,
            ProductName = "Monitor Pro",
            Description = "Pro Discount",
            Amount = 120
        }, null!);

        // Assert
        updated.ProductName.Should().Be("Monitor Pro");
        updated.Amount.Should().Be(120);
    }

    [Fact]
    public async Task Create_ThenDelete_CouponNoLongerExists()
    {
        // Arrange
        var context = CreateContext();
        var service = new DiscountService(context);

        var created = await service.CreateDiscount(new CreateDiscountRequest
        {
            ProductName = "Keyboard",
            Description = "Keyboard Discount",
            Amount = 20
        }, null!);

        // Act
        var deleteResult = await service.DeleteDiscount(new DeleteDiscountRequest { Id = created.Id }, null!);

        // Assert
        deleteResult.Success.Should().BeTrue();
        var fetched = await service.GetDiscount(new GetDiscountRequest { ProductName = "Keyboard" }, null!);
        fetched.ProductName.Should().Be("No Discount");
        fetched.Amount.Should().Be(0);
    }

    [Fact]
    public async Task FullCrudFlow_CreatesUpdatesAndDeletes()
    {
        // Arrange
        var context = CreateContext();
        var service = new DiscountService(context);

        // Create
        var created = await service.CreateDiscount(new CreateDiscountRequest
        {
            ProductName = "Tablet",
            Description = "Tablet Deal",
            Amount = 60
        }, null!);
        created.Id.Should().BeGreaterThan(0);

        // Update
        var updated = await service.UpdateDiscount(new UpdateDiscountRequest
        {
            Id = created.Id,
            ProductName = "Tablet Pro",
            Description = "Better Deal",
            Amount = 90
        }, null!);
        updated.ProductName.Should().Be("Tablet Pro");

        // Delete
        var deleted = await service.DeleteDiscount(new DeleteDiscountRequest { Id = created.Id }, null!);
        deleted.Success.Should().BeTrue();

        // Verify deleted
        var coupon = await context.Coupons.FindAsync(created.Id);
        coupon.Should().BeNull();
    }

    [Fact]
    public async Task DeleteNonExistent_ThrowsRpcNotFound()
    {
        // Arrange
        var context = CreateContext();
        var service = new DiscountService(context);

        // Act
        var act = async () => await service.DeleteDiscount(new DeleteDiscountRequest { Id = 9999 }, null!);

        // Assert
        await act.Should().ThrowAsync<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound);
    }
}
