using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CloudCart.Gateway.Tests;

public class GatewayRoutingTests : IAsyncLifetime
{
    private IHost? _catalogStub;
    private IHost? _basketStub;
    private IHost? _orderingStub;
    private IHost? _inventoryStub;
    private IHost? _discountStub;
    private IHost? _paymentStub;

    private WebApplicationFactory<Program> _gateway = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _catalogStub   = await StartStubAsync(15901, "catalog-ok");
        _basketStub    = await StartStubAsync(15902, "basket-ok");
        _orderingStub  = await StartStubAsync(15903, "ordering-ok");
        _inventoryStub = await StartStubAsync(15904, "inventory-ok");
        _discountStub  = await StartStubAsync(15905, "discount-ok");
        _paymentStub   = await StartStubAsync(15906, "payment-ok");

        var proxyConfig = new Dictionary<string, string?>
        {
            ["ReverseProxy:Routes:catalog-route:ClusterId"]                    = "catalog-cluster",
            ["ReverseProxy:Routes:catalog-route:Match:Path"]                   = "/catalog/{**catch-all}",
            ["ReverseProxy:Routes:catalog-route:Transforms:0:PathPattern"]     = "/{**catch-all}",

            ["ReverseProxy:Routes:basket-route:ClusterId"]                     = "basket-cluster",
            ["ReverseProxy:Routes:basket-route:Match:Path"]                    = "/basket/{**catch-all}",
            ["ReverseProxy:Routes:basket-route:Transforms:0:PathPattern"]      = "/{**catch-all}",

            ["ReverseProxy:Routes:ordering-route:ClusterId"]                   = "ordering-cluster",
            ["ReverseProxy:Routes:ordering-route:Match:Path"]                  = "/ordering/{**catch-all}",
            ["ReverseProxy:Routes:ordering-route:Transforms:0:PathPattern"]    = "/{**catch-all}",

            ["ReverseProxy:Routes:inventory-route:ClusterId"]                  = "inventory-cluster",
            ["ReverseProxy:Routes:inventory-route:Match:Path"]                 = "/inventory/{**catch-all}",
            ["ReverseProxy:Routes:inventory-route:Transforms:0:PathPattern"]   = "/{**catch-all}",

            ["ReverseProxy:Routes:discount-route:ClusterId"]                   = "discount-cluster",
            ["ReverseProxy:Routes:discount-route:Match:Path"]                  = "/discount/{**catch-all}",
            ["ReverseProxy:Routes:discount-route:Transforms:0:PathPattern"]    = "/{**catch-all}",

            ["ReverseProxy:Routes:payment-route:ClusterId"]                    = "payment-cluster",
            ["ReverseProxy:Routes:payment-route:Match:Path"]                   = "/payment/{**catch-all}",
            ["ReverseProxy:Routes:payment-route:Transforms:0:PathPattern"]     = "/{**catch-all}",

            ["ReverseProxy:Clusters:catalog-cluster:Destinations:d1:Address"]  = "http://localhost:15901",
            ["ReverseProxy:Clusters:basket-cluster:Destinations:d1:Address"]   = "http://localhost:15902",
            ["ReverseProxy:Clusters:ordering-cluster:Destinations:d1:Address"] = "http://localhost:15903",
            ["ReverseProxy:Clusters:inventory-cluster:Destinations:d1:Address"]= "http://localhost:15904",
            ["ReverseProxy:Clusters:discount-cluster:Destinations:d1:Address"] = "http://localhost:15905",
            ["ReverseProxy:Clusters:payment-cluster:Destinations:d1:Address"]  = "http://localhost:15906",

            // Keycloak — dummy authority za testove (JWT validacija nije cilj gateway routing testova)
            ["Keycloak:Authority"] = "http://localhost:19999/realms/test",
            ["Keycloak:Audience"]  = "test-client",
        };

        _gateway = new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
        {
            host.ConfigureAppConfiguration((_, cfg) =>
            {
                cfg.Sources.Clear();
                cfg.AddInMemoryCollection(proxyConfig);
            });
        });

        _client = _gateway.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _gateway.DisposeAsync();

        foreach (var stub in new[] { _catalogStub, _basketStub, _orderingStub, _inventoryStub, _discountStub, _paymentStub })
        {
            if (stub is not null)
            {
                await stub.StopAsync();
                stub.Dispose();
            }
        }
    }

    private static async Task<IHost> StartStubAsync(int port, string response)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web =>
            {
                web.UseUrls($"http://localhost:{port}");
                web.Configure(app =>
                {
                    app.Run(async ctx =>
                    {
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.WriteAsync(response);
                    });
                });
            })
            .Build();

        await host.StartAsync();
        return host;
    }

    [Fact]
    public async Task Request_ToCatalogRoute_ShouldForwardToCatalogService()
    {
        var response = await _client.GetAsync("/catalog/products");

        response.IsSuccessStatusCode.Should().BeTrue();
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("catalog-ok");
    }

    [Fact]
    public async Task Request_ToBasketRoute_ShouldForwardToBasketService()
    {
        var response = await _client.GetAsync("/basket/basket/john_doe");

        response.IsSuccessStatusCode.Should().BeTrue();
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("basket-ok");
    }

    [Fact]
    public async Task Request_ToOrderingRoute_ShouldForwardToOrderingService()
    {
        var response = await _client.GetAsync("/ordering/orders");

        response.IsSuccessStatusCode.Should().BeTrue();
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("ordering-ok");
    }

    [Fact]
    public async Task Request_ToInventoryRoute_ShouldForwardToInventoryService()
    {
        var response = await _client.GetAsync("/inventory/inventory");

        response.IsSuccessStatusCode.Should().BeTrue();
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("inventory-ok");
    }

    [Fact]
    public async Task Request_ToDiscountRoute_ShouldForwardToDiscountService()
    {
        var response = await _client.GetAsync("/discount/something");

        response.IsSuccessStatusCode.Should().BeTrue();
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("discount-ok");
    }

    [Fact]
    public async Task Request_ToPaymentRoute_ShouldForwardToPaymentService()
    {
        var response = await _client.GetAsync("/payment/create-intent");

        response.IsSuccessStatusCode.Should().BeTrue();
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("payment-ok");
    }

    [Fact]
    public async Task Request_ToUnknownRoute_ShouldReturn404()
    {
        var response = await _client.GetAsync("/unknown/something");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
