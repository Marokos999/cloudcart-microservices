using Inventory.Domain.Events;
using Inventory.Domain.ValueObjects;

namespace Inventory.Domain.Models;

public class InventoryItem : Aggregate<InventoryItemId>
{
    public ProductId ProductId { get; private set; } = default!;
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => Quantity - ReservedQuantity;

    private InventoryItem() { }

    public static InventoryItem Create(InventoryItemId id, ProductId productId, string productName, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name cannot be empty");
        if (quantity < 0)
            throw new DomainException("Quantity cannot be negative");

        var item = new InventoryItem
        {
            Id = id,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            ReservedQuantity = 0
        };

        item.AddDomainEvent(new InventoryItemCreatedEvent(item));
        return item;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");
        Quantity += quantity;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");
        if (quantity > AvailableQuantity)
            throw new DomainException($"Insufficient stock. Available: {AvailableQuantity}");
        Quantity -= quantity;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");
        if (quantity > AvailableQuantity)
            throw new DomainException($"Insufficient stock to reserve. Available: {AvailableQuantity}");
        ReservedQuantity += quantity;
    }
}