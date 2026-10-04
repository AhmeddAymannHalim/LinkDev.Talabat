using LinkDev.Talabat.Core.Application.Abstraction.Services;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Basket;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Orders;
using LinkDev.Talabat.Core.Application.Mapping;
using LinkDev.Talabat.Core.Application.Services.Basket;
using LinkDev.Talabat.Core.Application.Services.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace LinkDev.Talabat.Core.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(_ => { }, typeof(MappingProfile).Assembly);
            services.AddScoped(typeof(IServiceManager), typeof(ServiceManager));
            



            services.AddScoped(typeof(IBasketService),typeof(BasketService));

            services.AddScoped(typeof(Func<IBasketService>), (serviceProvider) =>
            {
                return () => serviceProvider.GetRequiredService<IBasketService>();
               
            });


            services.AddScoped(typeof(IOrderService),typeof(OrderService));
            services.AddScoped(typeof(Func<IOrderService>), (serviceProvider) =>
            {
                return () => serviceProvider.GetRequiredService<IOrderService>();

            });




            return services;
        }
    }
}
