using CloudCart.BuildingBlocks.Events;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.API.Email;
using Notification.API.EventHandlers;
using Notification.API.Hubs;
using Notification.API.Services;
using NSubstitute;

namespace Notification.API.IntegrationTests;

public class BasketCheckoutEventConsumerTests : IAsyncLifetime
{
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private IEmailService _emailService = null!;

    public async Task InitializeAsync()
    {
        _emailService = Substitute.For<IEmailService>();

        var hubContext = Substitute.For<IHubContext<NotificationHub>>();
        var clients = Substitute.For<IHubClients>();
        var clientProxy = Substitute.For<IClientProxy>();
        hubContext.Clients.Returns(clients);
        clients.Group(Arg.Any<string>()).Returns(clientProxy);

        _provider = new ServiceCollection()
            .AddSingleton(_emailService)
            .AddSingleton(hubContext)
            .AddSingleton<NotificationStore>()
            .AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<BasketCheckoutEventHandler>),
                NullLogger<BasketCheckoutEventHandler>.Instance)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<BasketCheckoutEventHandler>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    private BasketCheckoutEvent BuildEvent(string email = "test@example.com", decimal total = 99.99m) =>
        new()
        {
            UserName = "testuser",
            CustomerId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = email,
            AddressLine = "123 Main St",
            Country = "US",
            State = "NY",
            ZipCode = "10001",
            CardName = "John Doe",
            CardNumber = "4111111111111111",
            Expiration = "12/26",
            Cvv = "123",
            PaymentMethod = 1,
            TotalPrice = total
        };

    [Fact]
    public async Task Consumer_WhenBasketCheckoutEventPublished_IsConsumed()
    {
        // Arrange
        var @event = BuildEvent();

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        (await _harness.Consumed.Any<BasketCheckoutEvent>()).Should().BeTrue();
    }

    [Fact]
    public async Task Consumer_WhenBasketCheckoutEventPublished_CallsEmailService()
    {
        // Arrange
        var @event = BuildEvent("customer@example.com", 149.99m);

        // Act
        await _harness.Bus.Publish(@event);
        await _harness.Consumed.Any<BasketCheckoutEvent>();

        // Assert
        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.To == "customer@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consumer_EmailMessage_HasCorrectSubjectAndRecipient()
    {
        // Arrange
        var @event = BuildEvent("buyer@shop.com", 50m);

        // Act
        await _harness.Bus.Publish(@event);
        await _harness.Consumed.Any<BasketCheckoutEvent>();

        // Assert
        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m =>
                m.To == "buyer@shop.com" &&
                m.Subject == "CloudCart — Order Confirmation"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consumer_StoresNotificationForUser()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var @event = BuildEvent() with { CustomerId = customerId };
        var store = _provider.GetRequiredService<NotificationStore>();

        // Act
        await _harness.Bus.Publish(@event);
        await _harness.Consumed.Any<BasketCheckoutEvent>();

        // Assert
        var notifications = store.GetForUser(customerId.ToString());
        notifications.Should().NotBeEmpty();
        notifications[0].UserId.Should().Be(customerId.ToString());
    }
}
