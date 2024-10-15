using LinkDev.Talabat.Core.Domain.Contracts.Infrastructure;
using LinkDev.Talabat.Infrastructure.BasketRepositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace LinkDev.Talabat.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));

            services.AddSingleton(typeof(IConnectionMultiplexer), (_) =>
            {
                var connectionString = configuration.GetConnectionString("Redis");
                var  connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString!);
                return connectionMultiplexer;
            });

            services.AddScoped(typeof(IBasketRepository),typeof(BasketRepository));

            return services;
        }
    }
}
