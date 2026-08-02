using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CloudCart.Gateway.Tests;

public class GatewayAuthTests : IAsyncLifetime
{
    private IHost? _basketStub;
    private IHost? _orderingStub;
    private IHost? _inventoryStub;
    private IHost? _catalogStub;
    private IHost? _discountStub;
    private IHost? _paymentStub;

    private WebApplicationFactory<Program> _gateway = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _catalogStub   = await StartStubAsync(15911, "catalog-ok");
        _basketStub    = await StartStubAsync(15912, "basket-ok");
        _orderingStub  = await StartStubAsync(15913, "ordering-ok");
        _inventoryStub = await StartStubAsync(15914, "inventory-ok");
        _discountStub  = await StartStubAsync(15915, "discount-ok");
        _paymentStub   = await StartStubAsync(15916, "payment-ok");

        var proxyConfig = new Dictionary<string, string?>
        {
            ["ReverseProxy:Routes:catalog-route:ClusterId"]                     = "catalog-cluster",
            ["ReverseProxy:Routes:catalog-route:Match:Path"]                    = "/catalog/{**catch-all}",
            ["ReverseProxy:Routes:catalog-route:Transforms:0:PathPattern"]      = "/{**catch-all}",

            ["ReverseProxy:Routes:basket-route:ClusterId"]                      = "basket-cluster",
            ["ReverseProxy:Routes:basket-route:AuthorizationPolicy"]            = "Authenticated",
            ["ReverseProxy:Routes:basket-route:Match:Path"]                     = "/basket/{**catch-all}",
            ["ReverseProxy:Routes:basket-route:Transforms:0:PathPattern"]       = "/{**catch-all}",

            ["ReverseProxy:Routes:ordering-route:ClusterId"]                    = "ordering-cluster",
            ["ReverseProxy:Routes:ordering-route:AuthorizationPolicy"]          = "Authenticated",
            ["ReverseProxy:Routes:ordering-route:Match:Path"]                   = "/ordering/{**catch-all}",
            ["ReverseProxy:Routes:ordering-route:Transforms:0:PathPattern"]     = "/{**catch-all}",

            ["ReverseProxy:Routes:inventory-route:ClusterId"]                   = "inventory-cluster",
            ["ReverseProxy:Routes:inventory-route:AuthorizationPolicy"]         = "Authenticated",
            ["ReverseProxy:Routes:inventory-route:Match:Path"]                  = "/inventory/{**catch-all}",
            ["ReverseProxy:Routes:inventory-route:Transforms:0:PathPattern"]    = "/{**catch-all}",

            ["ReverseProxy:Routes:discount-route:ClusterId"]                    = "discount-cluster",
            ["ReverseProxy:Routes:discount-route:Match:Path"]                   = "/discount/{**catch-all}",
            ["ReverseProxy:Routes:discount-route:Transforms:0:PathPattern"]     = "/{**catch-all}",

            ["ReverseProxy:Routes:payment-route:ClusterId"]                     = "payment-cluster",
            ["ReverseProxy:Routes:payment-route:AuthorizationPolicy"]           = "Authenticated",
            ["ReverseProxy:Routes:payment-route:Match:Path"]                    = "/payment/{**catch-all}",
            ["ReverseProxy:Routes:payment-route:Transforms:0:PathPattern"]      = "/{**catch-all}",

            ["ReverseProxy:Clusters:catalog-cluster:Destinations:d1:Address"]   = "http://localhost:15911",
            ["ReverseProxy:Clusters:basket-cluster:Destinations:d1:Address"]    = "http://localhost:15912",
            ["ReverseProxy:Clusters:ordering-cluster:Destinations:d1:Address"]  = "http://localhost:15913",
            ["ReverseProxy:Clusters:inventory-cluster:Destinations:d1:Address"] = "http://localhost:15914",
            ["ReverseProxy:Clusters:discount-cluster:Destinations:d1:Address"]  = "http://localhost:15915",
            ["ReverseProxy:Clusters:payment-cluster:Destinations:d1:Address"]   = "http://localhost:15916",

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

            // Zameni JWT sa test auth shemom koja ne kontaktira Keycloak
            host.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                services.AddAuthorization(options =>
                    options.AddPolicy("Authenticated", policy =>
                        policy.RequireAuthenticatedUser()));
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
            if (stub is not null) { await stub.StopAsync(); stub.Dispose(); }
        }
    }

    private static async Task<IHost> StartStubAsync(int port, string response)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web =>
            {
                web.UseUrls($"http://localhost:{port}");
                web.Configure(app => app.Run(async ctx =>
                {
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.WriteAsync(response);
                }));
            })
            .Build();

        await host.StartAsync();
        return host;
    }

    // ── Catalog (public) ──────────────────────────────────────────────────────

    [Fact]
    public async Task Catalog_WithoutToken_ShouldReturn200()
    {
        var response = await _client.GetAsync("/catalog/products");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Catalog_WithToken_ShouldReturn200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "valid");

        var response = await _client.GetAsync("/catalog/products");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    // ── Basket (zaštićen) ─────────────────────────────────────────────────────

    [Fact]
    public async Task Basket_WithoutToken_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/basket/basket/testuser");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Basket_WithToken_ShouldReturn200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "valid");

        var response = await _client.GetAsync("/basket/basket/testuser");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    // ── Ordering (zaštićen) ───────────────────────────────────────────────────

    [Fact]
    public async Task Ordering_WithoutToken_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/ordering/orders");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Ordering_WithToken_ShouldReturn200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "valid");

        var response = await _client.GetAsync("/ordering/orders");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    // ── Inventory (zaštićen) ──────────────────────────────────────────────────

    [Fact]
    public async Task Inventory_WithoutToken_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/inventory/inventory");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Inventory_WithToken_ShouldReturn200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "valid");

        var response = await _client.GetAsync("/inventory/inventory");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    // ── Discount (javan) ──────────────────────────────────────────────────────

    [Fact]
    public async Task Discount_WithoutToken_ShouldReturn200()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/discount/something");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Discount_WithToken_ShouldReturn200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "valid");

        var response = await _client.GetAsync("/discount/something");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    // ── Payment (zaštićen) ────────────────────────────────────────────────────

    [Fact]
    public async Task Payment_WithoutToken_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/payment/create-intent");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Payment_WithToken_ShouldReturn200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "valid");

        var response = await _client.GetAsync("/payment/create-intent");

        response.IsSuccessStatusCode.Should().BeTrue();
    }
}

// Fake auth handler — autentifikuje svaki zahtev koji ima Authorization header
public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("No Authorization header"));

        var claims = new[] { new Claim(ClaimTypes.Name, "testuser"), new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
