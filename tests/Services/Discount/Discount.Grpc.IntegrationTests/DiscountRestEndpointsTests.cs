using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CloudCart.BuildingBlocks.Events;
using Discount.Grpc.Data;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Discount.Grpc.IntegrationTests;

public class DiscountRestEndpointsTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string _dbPath = "";
    public IPublishEndpoint PublishEndpoint { get; private set; } = null!;

    public Task InitializeAsync()
    {
        PublishEndpoint = Substitute.For<IPublishEndpoint>();
        _dbPath = Path.Combine(Path.GetTempPath(), $"discount_test_{Guid.NewGuid()}.db");

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace SQLite connection string (same provider, isolated temp file)
                services.RemoveAll(typeof(DbContextOptions<DiscountContext>));
                services.AddDbContext<DiscountContext>(opt =>
                    opt.UseSqlite($"Data Source={_dbPath}"));

                // Remove all hosted services (prevents MassTransit from connecting to RabbitMQ)
                var hostedServices = services
                    .Where(d => d.ServiceType == typeof(IHostedService) ||
                                d.ServiceType == typeof(IHostedLifecycleService))
                    .ToList();
                foreach (var hs in hostedServices) services.Remove(hs);

                // Replace IPublishEndpoint with mock
                services.RemoveAll(typeof(IPublishEndpoint));
                services.AddSingleton(PublishEndpoint);
            });
        });

        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* SQLite may still hold a lock */ }
    }

    [Fact]
    public async Task GetCoupons_ReturnsOk_WithSeededCoupons()
    {
        var response = await _client.GetAsync("/coupons");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PostCoupon_ReturnsCreated_WithAssignedId()
    {
        var coupon = new { productName = "Test Product", description = "Test", amount = 50, code = "" };

        var response = await _client.PostAsJsonAsync("/coupons", coupon);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetInt32().Should().BeGreaterThan(0);
        body.GetProperty("productName").GetString().Should().Be("Test Product");
    }

    [Fact]
    public async Task PostCoupon_ThenGetCoupons_ReturnsCoupon()
    {
        var coupon = new { productName = "Nintendo Switch", description = "Deal", amount = 30, code = "SWITCH30" };
        await _client.PostAsJsonAsync("/coupons", coupon);

        var response = await _client.GetAsync("/coupons");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetArrayLength().Should().BeGreaterThan(0);
        body[0].GetProperty("productName").GetString().Should().Be("Nintendo Switch");
    }

    [Fact]
    public async Task PostCoupon_PublishesCouponCreatedEvent()
    {
        var coupon = new { productName = "Xbox", description = "Xbox Deal", amount = 75, code = "XBOX75" };

        await _client.PostAsJsonAsync("/coupons", coupon);

        await PublishEndpoint.Received(1).Publish(
            Arg.Is<CouponCreatedEvent>(e => e.ProductName == "Xbox" && e.Code == "XBOX75"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PostCoupon_ForcesIdToZero_PreventsIdInjection()
    {
        var coupon = new { id = 9999, productName = "Injected", description = "Attempt", amount = 1, code = "" };

        var response = await _client.PostAsJsonAsync("/coupons", coupon);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetInt32().Should().NotBe(9999);
    }
}
