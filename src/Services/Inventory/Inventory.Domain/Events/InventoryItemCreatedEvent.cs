using MediatR;
using Inventory.Domain.Models;

namespace Inventory.Domain.Events;

public record InventoryItemCreatedEvent(InventoryItem Item) : INotification;