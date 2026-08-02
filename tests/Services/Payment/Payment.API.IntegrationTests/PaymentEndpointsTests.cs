using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Payment.API;

namespace Payment.API.IntegrationTests;

public class PaymentEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly IStripeService _stripeService;

    public PaymentEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _stripeService = Substitute.For<IStripeService>();

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IStripeService));
                if (descriptor != null) services.Remove(descriptor);

                services.AddScoped<IStripeService>(_ => _stripeService);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task CreateIntent_ShouldReturn200_WithClientSecretAndPaymentIntentId()
    {
        // Arrange
        _stripeService
            .CreateIntentAsync(Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(("pi_test_secret_123", "pi_test_123"));

        var request = new { Amount = 99.99m, CustomerId = "cust_001", OrderName = "Order-001" };

        // Act
        var response = await _client.PostAsJsonAsync("/create-intent", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("clientSecret").GetString().Should().Be("pi_test_secret_123");
        body.GetProperty("paymentIntentId").GetString().Should().Be("pi_test_123");
    }

    [Fact]
    public async Task CreateIntent_ShouldCallStripeService_WithCorrectParameters()
    {
        // Arrange
        _stripeService
            .CreateIntentAsync(Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(("secret", "pi_id"));

        var request = new { Amount = 149.50m, CustomerId = "cust_002", OrderName = "Order-002" };

        // Act
        await _client.PostAsJsonAsync("/create-intent", request);

        // Assert
        await _stripeService.Received(1)
            .CreateIntentAsync(149.50m, "cust_002", "Order-002");
    }

    [Fact]
    public async Task Webhook_ShouldReturn200_WhenEventParsedSuccessfully()
    {
        // Arrange
        var fakeEvent = new Stripe.Event { Id = "evt_test_001", Type = "charge.succeeded" };
        _stripeService
            .ParseWebhookEvent(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(fakeEvent);

        var content = new StringContent("{\"id\":\"evt_test_001\"}", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/webhook", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Webhook_ShouldReturn400_WhenParsingFails()
    {
        // Arrange
        _stripeService
            .ParseWebhookEvent(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(_ => throw new Exception("Invalid JSON"));

        var content = new StringContent("not-valid-json", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/webhook", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateIntent_ShouldReturn200_WithZeroAmount()
    {
        // Arrange
        _stripeService
            .CreateIntentAsync(0m, Arg.Any<string>(), Arg.Any<string>())
            .Returns(("secret_zero", "pi_zero"));

        var request = new { Amount = 0m, CustomerId = "cust_003", OrderName = "Order-003" };

        // Act
        var response = await _client.PostAsJsonAsync("/create-intent", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
