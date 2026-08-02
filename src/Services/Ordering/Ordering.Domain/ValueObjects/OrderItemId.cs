namespace Ordering.Domain.ValueObjects;

public record OrderItemId(Guid Value)
{
    public static OrderItemId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("OrderItemId cannot be empty.");
        return new OrderItemId(value);
    }
}