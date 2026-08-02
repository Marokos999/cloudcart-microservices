using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

public class DiscountService(DiscountContext context) : DiscountProtoService.DiscountProtoServiceBase
{
    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext callContext)
    {
        var coupon = await context.Coupons
            .FirstOrDefaultAsync(c => c.ProductName == request.ProductName);

        if (coupon is null)
            coupon = new Coupon { ProductName = "No Discount", Description = "No Discount", Amount = 0 };

        return new CouponModel
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName,
            Description = coupon.Description,
            Amount = coupon.Amount
        };
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext callContext)
    {
        var coupon = new Coupon
        {
            ProductName = request.ProductName,
            Description = request.Description,
            Amount = request.Amount
        };

        context.Coupons.Add(coupon);
        await context.SaveChangesAsync();

        return new CouponModel
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName,
            Description = coupon.Description,
            Amount = coupon.Amount
        };
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext callContext)
    {
        var coupon = await context.Coupons.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with id {request.Id} not found."));

        coupon.ProductName = request.ProductName;
        coupon.Description = request.Description;
        coupon.Amount = request.Amount;

        await context.SaveChangesAsync();

        return new CouponModel
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName,
            Description = coupon.Description,
            Amount = coupon.Amount
        };
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext callContext)
    {
        var coupon = await context.Coupons.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with id {request.Id} not found."));

        context.Coupons.Remove(coupon);
        await context.SaveChangesAsync();

        return new DeleteDiscountResponse { Success = true };
    }

     
}
