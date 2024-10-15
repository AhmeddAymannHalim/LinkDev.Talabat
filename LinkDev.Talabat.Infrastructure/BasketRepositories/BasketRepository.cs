using LinkDev.Talabat.Core.Domain.Contracts.Infrastructure;
using LinkDev.Talabat.Core.Domain.Entities.Basket;
using StackExchange.Redis;
using System.Text.Json;

namespace LinkDev.Talabat.Infrastructure.BasketRepositories;

public class BasketRepository : IBasketRepository
{
    private readonly IDatabase _database;

    public BasketRepository(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<CustomerBasket?> GetBasketAsync(string id)
    {
        var basket = await _database.StringGetAsync(id);

        return basket.IsNullOrEmpty ? null : JsonSerializer.Deserialize<CustomerBasket>(basket!);
    }

    public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket , TimeSpan timeToLive)
    {
        var value = JsonSerializer.Serialize(basket);


        var updated = await _database.StringSetAsync(basket.Id, value , timeToLive);

        if (updated) return basket;

        return null;
    }

    public async Task<bool> DeleteBasketAsync(string id) =>  await _database.KeyDeleteAsync(id);
    
}
