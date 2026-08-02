namespace Discount.Grpc.Models;

public class Coupon
{
    public int Id { get; set; }
    public string ProductName { get; set; } = "";
    public string Description { get; set; } = "";
    public int Amount { get; set; }

    // Promo code (e.g. SUMMER50). Empty for automatic per-product discounts;
    // set for coupons the customer applies to the whole cart at checkout.
    public string Code { get; set; } = "";
}