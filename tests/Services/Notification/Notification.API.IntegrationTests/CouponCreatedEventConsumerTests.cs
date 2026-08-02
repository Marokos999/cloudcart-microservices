using CloudCart.BuildingBlocks.Events;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.API.EventHandlers;
using Notification.API.Hubs;
using Notification.API.Services;
using NSubstitute;

namespace Notification.API.IntegrationTests;

public class CouponCreatedEventConsumerTests : IAsyncLifetime
{
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;

    public async Task InitializeAsync()
    {
        var hubContext = Substitute.For<IHubContext<NotificationHub>>();
        var clients = Substitute.For<IHubClients>();
        var clientProxy = Substitute.For<IClientProxy>();
        hubContext.Clients.Returns(clients);
        clients.All.Returns(clientProxy);

        _provider = new ServiceCollection()
            .AddSingleton(hubContext)
            .AddSingleton<NotificationStore>()
            .AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<CouponCreatedEventHandler>),
                NullLogger<CouponCreatedEventHandler>.Instance)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<CouponCreatedEventHandler>();
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

    private static CouponCreatedEvent BuildEvent(string code = "") => new()
    {
        CouponId = 1,
        ProductName = "PlayStation 5",
        Description = "PS5 Discount",
        Amount = 200,
        Code = code
    };

    [Fact]
    public async Task Consumer_WhenCouponCreatedEventPublished_IsConsumed()
    {
        await _harness.Bus.Publish(BuildEvent());

        (await _harness.Consumed.Any<CouponCreatedEvent>()).Should().BeTrue();
    }

    [Fact]
    public async Task Consumer_StoresBroadcastNotification()
    {
        var store = _provider.GetRequiredService<NotificationStore>();

        await _harness.Bus.Publish(BuildEvent());
        await _harness.Consumed.Any<CouponCreatedEvent>();

        store.GetForUser("admin").Should().NotBeEmpty();
    }

    [Fact]
    public async Task Consumer_NotificationText_ContainsProductName()
    {
        var store = _provider.GetRequiredService<NotificationStore>();

        await _harness.Bus.Publish(BuildEvent());
        await _harness.Consumed.Any<CouponCreatedEvent>();

        store.GetForUser("admin")[0].Text.Should().Contain("PlayStation 5");
    }

    [Fact]
    public async Task Consumer_NotificationText_ContainsCode_WhenProvided()
    {
        var store = _provider.GetRequiredService<NotificationStore>();

        await _harness.Bus.Publish(BuildEvent("SAVE200"));
        await _harness.Consumed.Any<CouponCreatedEvent>();

        store.GetForUser("admin")[0].Text.Should().Contain("SAVE200");
    }
}
