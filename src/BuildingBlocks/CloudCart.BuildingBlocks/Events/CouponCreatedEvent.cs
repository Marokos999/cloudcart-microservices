namespace CloudCart.BuildingBlocks.Events;

public record CouponCreatedEvent
{
    public int CouponId { get; init; }
    public string ProductName { get; init; } = default!;
    public string Description { get; init; } = default!;
    public int Amount { get; init; }
    public string Code { get; init; } = "";
}
