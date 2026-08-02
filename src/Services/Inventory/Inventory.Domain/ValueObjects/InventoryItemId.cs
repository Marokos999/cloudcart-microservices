namespace Inventory.Domain.ValueObjects;

public record InventoryItemId
{
    public Guid Value { get; }
    private InventoryItemId(Guid value) => Value = value;
    public static InventoryItemId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("InventoryItemId cannot be empty");
        return new InventoryItemId(value);
    }
}