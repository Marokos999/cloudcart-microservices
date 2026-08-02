namespace Ordering.Domain.ValueObjects;

public record OrderName(string Value)
{
    public static OrderName Of(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("OrderName cannot be empty.");
        return new OrderName(value);
    }
}