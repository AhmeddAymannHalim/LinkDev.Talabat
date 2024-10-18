using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Auth;
using LinkDev.Talabat.Core.Application.Abstraction.Services;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Auth;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Basket;
using LinkDev.Talabat.Core.Application.Mapping;
using LinkDev.Talabat.Core.Application.Services.Auth;
using LinkDev.Talabat.Core.Application.Services.Basket;
using LinkDev.Talabat.Core.Domain.Contracts.Infrastructure;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LinkDev.Talabat.Core.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddScoped(typeof(IServiceManager), typeof(ServiceManager));
            
            //services.AddScoped(typeof(IBasketService),typeof(BasketService));
            //services.AddScoped(typeof(Func<IBasketService>),typeof(Func<BasketService>));

            services.AddScoped(typeof(Func<IBasketService>), (serviceProvider) =>
            {
                var mapper = serviceProvider.GetRequiredService<IMapper>();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var basketRepository = serviceProvider.GetRequiredService<IBasketRepository>();

                return () => new BasketService(basketRepository, mapper, configuration);
               
            });

           



            return services;
        }
    }
}
