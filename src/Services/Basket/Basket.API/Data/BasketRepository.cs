using Basket.API.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Basket.API.Data;

public class BasketRepository(IConnectionMultiplexer redis) : IBasketRepository
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
    {
        var data = await _db.StringGetAsync(userName);
        return data.IsNullOrEmpty
            ? new ShoppingCart(userName)
            : JsonSerializer.Deserialize<ShoppingCart>((string)data!)!;
    }

    public async Task<ShoppingCart> StoreBasket(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        await _db.StringSetAsync(cart.UserName, JsonSerializer.Serialize(cart));
        return cart;
    }

    public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
    {
        return await _db.KeyDeleteAsync(userName);
    }
}
