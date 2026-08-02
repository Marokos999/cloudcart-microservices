using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Payment.API;
using System.Net;
using System.Text;

namespace Payment.API.UnitTests;

public class PaymentWebhookTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PaymentWebhookTests(WebApplicationFactory<Program> factory)
    {
        var mockStripe = Substitute.For<IStripeService>();
        mockStripe.ParseWebhookEvent(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(new Stripe.Event { Id = "evt_test", Type = "charge.succeeded" });

        _factory = factory.WithWebHostBuilder(host =>
        {
            host.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStripeService));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddScoped(_ => mockStripe);
            });
        });
    }

    [Fact]
    public async Task Webhook_WithValidEvent_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var json = """{"type":"charge.succeeded","data":{"object":{}}}""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/webhook", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Webhook_WhenParsingThrows_ReturnsBadRequest()
    {
        // Arrange — mock throws to simulate invalid signature / bad payload
        var throwingStripe = Substitute.For<IStripeService>();
        throwingStripe.ParseWebhookEvent(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(_ => throw new Exception("Invalid webhook"));

        var factoryWithThrow = _factory.WithWebHostBuilder(host =>
        {
            host.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStripeService));
                if (descriptor is not null) services.Remove(descriptor);
                services.AddScoped(_ => throwingStripe);
            });
        });

        var client = factoryWithThrow.CreateClient();
        var content = new StringContent("""{"type":"test"}""", Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/webhook", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
