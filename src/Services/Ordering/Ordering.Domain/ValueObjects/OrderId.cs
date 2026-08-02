namespace Ordering.Domain.ValueObjects;

public record OrderId(Guid Value)
{
    public static OrderId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("OrderId cannot be empty.");
        return new OrderId(value);
    }
}
