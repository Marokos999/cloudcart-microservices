namespace Inventory.Application.Dtos;

public record InventoryItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    int ReservedQuantity,
    int AvailableQuantity);