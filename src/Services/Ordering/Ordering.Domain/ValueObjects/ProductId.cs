namespace Ordering.Domain.ValueObjects;

public record ProductId(Guid Value)
{
    public static ProductId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("ProductId cannot be empty.");
        return new ProductId(value);
    }
}
