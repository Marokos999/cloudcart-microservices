using CloudCart.BuildingBlocks.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Notification.API.Hubs;
using Notification.API.Models;
using Notification.API.Services;

namespace Notification.API.EventHandlers;

public class CouponCreatedEventHandler(
    IHubContext<NotificationHub> hub,
    NotificationStore store,
    ILogger<CouponCreatedEventHandler> logger)
    : IConsumer<CouponCreatedEvent>
{
    public async Task Consume(ConsumeContext<CouponCreatedEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("Coupon created: {ProductName} -{Amount}$", msg.ProductName, msg.Amount);

        var text = msg.Code != ""
            ? $"New coupon created: code \"{msg.Code}\" — ${msg.Amount} off {msg.ProductName}"
            : $"New coupon created: ${msg.Amount} off {msg.ProductName}";

        var notification = new AppNotification(
            Id: Guid.NewGuid().ToString(),
            UserId: "admin",
            Icon: "🎟️",
            Text: text,
            CreatedAt: DateTime.UtcNow.ToString("o"),
            Unread: true,
            Link: "/admin/coupons");

        store.AddBroadcast(notification);
        await hub.Clients.All.SendAsync("ReceiveNotification", notification);
    }
}
