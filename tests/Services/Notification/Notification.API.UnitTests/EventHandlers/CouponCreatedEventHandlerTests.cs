using CloudCart.BuildingBlocks.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.API.EventHandlers;
using Notification.API.Hubs;
using Notification.API.Services;
using NSubstitute;

namespace Notification.API.UnitTests.EventHandlers;

public class CouponCreatedEventHandlerTests
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IClientProxy _clientProxy;
    private readonly NotificationStore _store;
    private readonly CouponCreatedEventHandler _handler;

    public CouponCreatedEventHandlerTests()
    {
        _hub = Substitute.For<IHubContext<NotificationHub>>();
        var clients = Substitute.For<IHubClients>();
        _clientProxy = Substitute.For<IClientProxy>();
        _hub.Clients.Returns(clients);
        clients.All.Returns(_clientProxy);

        _store = new NotificationStore();
        _handler = new CouponCreatedEventHandler(_hub, _store, NullLogger<CouponCreatedEventHandler>.Instance);
    }

    private static CouponCreatedEvent BuildEvent(string productName = "PlayStation 5", int amount = 100, string code = "") =>
        new() { CouponId = 1, ProductName = productName, Description = "Test", Amount = amount, Code = code };

    private static ConsumeContext<CouponCreatedEvent> WrapEvent(CouponCreatedEvent e)
    {
        var ctx = Substitute.For<ConsumeContext<CouponCreatedEvent>>();
        ctx.Message.Returns(e);
        return ctx;
    }

    [Fact]
    public async Task Consume_SendsSignalRNotification()
    {
        await _handler.Consume(WrapEvent(BuildEvent()));

        await _clientProxy.Received(1).SendCoreAsync(
            "ReceiveNotification", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_StoresNotificationAsBroadcast()
    {
        await _handler.Consume(WrapEvent(BuildEvent("Xbox Series X", 150)));

        var notifications = _store.GetForUser("admin");
        notifications.Should().NotBeEmpty();
        notifications[0].Text.Should().Contain("Xbox Series X");
    }

    [Fact]
    public async Task Consume_TextIncludesCode_WhenCodeProvided()
    {
        await _handler.Consume(WrapEvent(BuildEvent(code: "SUMMER50")));

        var notifications = _store.GetForUser("any");
        notifications[0].Text.Should().Contain("SUMMER50");
    }

    [Fact]
    public async Task Consume_TextExcludesCode_WhenCodeEmpty()
    {
        await _handler.Consume(WrapEvent(BuildEvent(code: "")));

        var notifications = _store.GetForUser("any");
        notifications[0].Text.Should().NotContain("code");
    }

    [Fact]
    public async Task Consume_NotificationHasAdminLink()
    {
        await _handler.Consume(WrapEvent(BuildEvent()));

        var notifications = _store.GetForUser("admin");
        notifications[0].Link.Should().Be("/admin/coupons");
    }

    [Fact]
    public async Task Consume_NotificationIsUnread()
    {
        await _handler.Consume(WrapEvent(BuildEvent()));

        var notifications = _store.GetForUser("admin");
        notifications[0].Unread.Should().BeTrue();
    }
}
