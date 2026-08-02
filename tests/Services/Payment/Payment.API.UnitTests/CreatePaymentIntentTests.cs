using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Payment.API;
using System.Net;
using System.Net.Http.Json;

namespace Payment.API.UnitTests;

public class CreatePaymentIntentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CreatePaymentIntentTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private WebApplicationFactory<Program> BuildFactory(IStripeService stripeService)
    {
        return _factory.WithWebHostBuilder(host =>
        {
            host.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStripeService));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddScoped(_ => stripeService);
            });
        });
    }

    [Fact]
    public async Task CreateIntent_WithValidRequest_Returns200WithClientSecret()
    {
        // Arrange
        var stripeService = Substitute.For<IStripeService>();
        stripeService
            .CreateIntentAsync(99.99m, "cust_123", "Order-1")
            .Returns(("secret_abc123", "pi_abc123"));

        var client = BuildFactory(stripeService).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/create-intent", new
        {
            Amount = 99.99m,
            CustomerId = "cust_123",
            OrderName = "Order-1"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CreateIntentResponse>();
        body!.clientSecret.Should().Be("secret_abc123");
        body.paymentIntentId.Should().Be("pi_abc123");
    }

    [Fact]
    public async Task CreateIntent_CallsStripeService_WithCorrectParameters()
    {
        // Arrange
        var stripeService = Substitute.For<IStripeService>();
        stripeService
            .CreateIntentAsync(Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(("secret_xyz", "pi_xyz"));

        var client = BuildFactory(stripeService).CreateClient();

        // Act
        await client.PostAsJsonAsync("/create-intent", new
        {
            Amount = 250m,
            CustomerId = "cust_456",
            OrderName = "Order-2"
        });

        // Assert
        await stripeService.Received(1).CreateIntentAsync(250m, "cust_456", "Order-2");
    }

    private record CreateIntentResponse(string clientSecret, string paymentIntentId);
}
