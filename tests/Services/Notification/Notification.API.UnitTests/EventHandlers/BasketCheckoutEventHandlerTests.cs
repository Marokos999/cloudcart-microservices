#pragma warning disable CS8619
#pragma warning disable CS8602
using CloudCart.BuildingBlocks.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.API.Email;
using Notification.API.EventHandlers;
using Notification.API.Hubs;
using Notification.API.Services;
using NSubstitute;

namespace Notification.API.UnitTests.EventHandlers;

public class BasketCheckoutEventHandlerTests
{
    private readonly IEmailService _emailService;
    private readonly BasketCheckoutEventHandler _handler;

    public BasketCheckoutEventHandlerTests()
    {
        _emailService = Substitute.For<IEmailService>();
        var hub = Substitute.For<IHubContext<NotificationHub>>();
        var clients = Substitute.For<IHubClients>();
        var clientProxy = Substitute.For<IClientProxy>();
        hub.Clients.Returns(clients);
        clients.Group(Arg.Any<string>()).Returns(clientProxy);
        var store = new NotificationStore();

        _handler = new BasketCheckoutEventHandler(
            _emailService, hub, store, NullLogger<BasketCheckoutEventHandler>.Instance);
    }

    private static BasketCheckoutEvent BuildEvent(string email = "john@example.com") => new()
    {
        UserName = "john_doe",
        CustomerId = Guid.NewGuid(),
        FirstName = "John",
        LastName = "Doe",
        EmailAddress = email,
        AddressLine = "123 Main St",
        Country = "USA",
        State = "NY",
        ZipCode = "10001",
        CardName = "John Doe",
        CardNumber = "4111111111111111",
        Expiration = "12/26",
        Cvv = "123",
        PaymentMethod = 1,
        TotalPrice = 499.99m
    };

    [Fact]
    public async Task Consume_ShouldSendEmail_ToCorrectAddress()
    {
        var context = Substitute.For<ConsumeContext<BasketCheckoutEvent>>();
        context.Message.Returns(BuildEvent("john@example.com"));

        await _handler.Consume(context);

        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.To == "john@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldSendEmail_WithOrderConfirmationSubject()
    {
        var context = Substitute.For<ConsumeContext<BasketCheckoutEvent>>();
        context.Message.Returns(BuildEvent());

        await _handler.Consume(context);

        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.Subject.Contains("Order Confirmation")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldSendEmail_WithCustomerNameInBody()
    {
        var context = Substitute.For<ConsumeContext<BasketCheckoutEvent>>();
        context.Message.Returns(BuildEvent());

        await _handler.Consume(context);

        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.Body.Contains("John")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldSendEmail_WithTotalPriceInBody()
    {
        var context = Substitute.For<ConsumeContext<BasketCheckoutEvent>>();
        context.Message.Returns(BuildEvent());

        await _handler.Consume(context);

        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.Body.Contains("499.99")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldCallEmailService_ExactlyOnce()
    {
        var context = Substitute.For<ConsumeContext<BasketCheckoutEvent>>();
        context.Message.Returns(BuildEvent());

        await _handler.Consume(context);

        await _emailService.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }
}
