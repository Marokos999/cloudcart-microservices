using Basket.API.Data;
using Basket.API.Features.DeleteBasket;
using Basket.API.Features.GetBasket;
using Basket.API.Models;
using FluentAssertions;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace Basket.API.IntegrationTests.Features;

public class BasketIntegrationTests : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7.4-alpine")
        .Build();

    private IBasketRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();

        var multiplexer = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
        _repository = new BasketRepository(multiplexer);
    }

    public async Task DisposeAsync()
    {
        await _redisContainer.StopAsync();
    }

    [Fact]
    public async Task StoreBasket_ThenGetBasket_ShouldReturnSameCart()
    {
        var cart = new ShoppingCart("john_doe")
        {
            Items =
            [
                new ShoppingCartItem { ProductId = "1", ProductName = "PlayStation 5", Price = 499.99m, Quantity = 1 },
                new ShoppingCartItem { ProductId = "2", ProductName = "Xbox Series X", Price = 449.99m, Quantity = 2 }
            ]
        };

        await _repository.StoreBasket(cart);

        var getHandler = new GetBasketHandler(_repository);
        var result = await getHandler.Handle(new GetBasketQuery("john_doe"), CancellationToken.None);

        result.Cart.UserName.Should().Be("john_doe");
        result.Cart.Items.Should().HaveCount(2);
        result.Cart.TotalPrice.Should().Be(499.99m + 449.99m * 2);
    }

    [Fact]
    public async Task GetBasket_ShouldReturnEmptyCart_WhenNotExists()
    {
        var handler = new GetBasketHandler(_repository);
        var result = await handler.Handle(new GetBasketQuery("nonexistent"), CancellationToken.None);

        result.Cart.UserName.Should().Be("nonexistent");
        result.Cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteBasket_ShouldReturnTrue_WhenCartExists()
    {
        var cart = new ShoppingCart("to_delete");
        await _repository.StoreBasket(cart);

        var handler = new DeleteBasketHandler(_repository);
        var result = await handler.Handle(new DeleteBasketCommand("to_delete"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteBasket_ThenGet_ShouldReturnEmptyCart()
    {
        var cart = new ShoppingCart("temp_user")
        {
            Items = [new ShoppingCartItem { ProductId = "1", ProductName = "Nintendo Switch OLED", Price = 349.99m, Quantity = 1 }]
        };
        await _repository.StoreBasket(cart);

        await _repository.DeleteBasket("temp_user");

        var getHandler = new GetBasketHandler(_repository);
        var result = await getHandler.Handle(new GetBasketQuery("temp_user"), CancellationToken.None);

        result.Cart.Items.Should().BeEmpty();
    }
}
