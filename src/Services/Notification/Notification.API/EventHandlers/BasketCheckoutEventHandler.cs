using CloudCart.BuildingBlocks.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Notification.API.Email;
using Notification.API.Hubs;
using Notification.API.Models;
using Notification.API.Services;

namespace Notification.API.EventHandlers;

public class BasketCheckoutEventHandler(
    IEmailService emailService,
    IHubContext<NotificationHub> hub,
    NotificationStore store,
    ILogger<BasketCheckoutEventHandler> logger)
    : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Received BasketCheckoutEvent for user {UserName}", message.UserName);

        // Save and push in-app notification
        var notification = new AppNotification(
            Id: Guid.NewGuid().ToString(),
            UserId: message.CustomerId.ToString(),
            Icon: "📦",
            Text: $"Order placed successfully! Total: ${message.TotalPrice:F2}",
            CreatedAt: DateTime.UtcNow.ToString("o"),
            Unread: true,
            Link: "/profile");

        var userId = message.CustomerId.ToString();
        store.Add(userId, notification);
        await hub.Clients.Group(userId).SendAsync("ReceiveNotification", notification);

        // Send confirmation email
        var email = new EmailMessage(
            To: message.EmailAddress,
            Subject: "CloudCart — Order Confirmation",
            Body: $"""
                <h2>Thank you for your order, {message.FirstName}!</h2>
                <p>Your order has been placed successfully.</p>
                <table>
                    <tr><td><strong>Customer:</strong></td><td>{message.FirstName} {message.LastName}</td></tr>
                    <tr><td><strong>Email:</strong></td><td>{message.EmailAddress}</td></tr>
                    <tr><td><strong>Total:</strong></td><td>${message.TotalPrice:F2}</td></tr>
                    <tr><td><strong>Shipping to:</strong></td><td>{message.AddressLine}, {message.State}, {message.Country} {message.ZipCode}</td></tr>
                </table>
                <p>We will notify you when your order ships.</p>
                <p>— CloudCart Team</p>
                """);

        await emailService.SendAsync(email, context.CancellationToken);
    }
}
