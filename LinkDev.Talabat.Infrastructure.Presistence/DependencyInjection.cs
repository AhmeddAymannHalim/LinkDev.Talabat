﻿using LinkDev.Talabat.Core.Domain.Contracts.Infrastructure;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Infrastructure.Presistence._Identity;
using LinkDev.Talabat.Infrastructure.Presistence.Data;
using LinkDev.Talabat.Infrastructure.Presistence.Data.Interceptors;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace LinkDev.Talabat.Infrastructure.Presistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services,IConfiguration configuration)
        {
            #region StoreDbContext 
            services.AddDbContext<StoreDbContext>(optionsBuilder =>
             {
                 optionsBuilder.UseLazyLoadingProxies();
                 optionsBuilder.UseSqlServer(configuration.GetConnectionString("StoreContext"));
             });


            // services.AddScoped<IStoreContextInitializer, StoreContextInitializer>();
            services.AddScoped(typeof(IStoreContextInitializer), typeof(StoreDbContextInitializer));

            services.AddScoped(typeof(ISaveChangesInterceptor), typeof(BaseAuditableEntityInterceptor));

            #endregion


            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork.UnitOfWork));


            #region IdentityDbContext
            services.AddDbContext<StoreIdentityDbContext>(optionsBuilder =>
               {
                   optionsBuilder.UseLazyLoadingProxies();
                   optionsBuilder.UseSqlServer(configuration.GetConnectionString("IdentityContext"));
               });

            #endregion

            return services;
        }
    }
}
