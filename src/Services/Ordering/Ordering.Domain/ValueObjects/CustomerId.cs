namespace Ordering.Domain.ValueObjects;

public record CustomerId(Guid Value)
{
    public static CustomerId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("CustomerId cannot be empty.");
        return new CustomerId(value);
    }
}
