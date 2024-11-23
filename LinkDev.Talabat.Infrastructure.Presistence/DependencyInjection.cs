using LinkDev.Talabat.Core.Domain.Contracts.Infrastructure;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence.DbInitializers;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using LinkDev.Talabat.Infrastructure.Presistence._Identity;
using LinkDev.Talabat.Infrastructure.Presistence.Data;
using LinkDev.Talabat.Infrastructure.Presistence.Data.Interceptors;
using Microsoft.AspNetCore.Identity;
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
            services.AddDbContext<StoreDbContext>((serviceProvdider, optionsBuilder) =>
             {
                 optionsBuilder.UseLazyLoadingProxies();
                 optionsBuilder.UseSqlServer(configuration.GetConnectionString("StoreContext"))
                 .AddInterceptors(serviceProvdider.GetRequiredService<AuditInterceptor>());
             });

            services.AddScoped(typeof(IStoreDbInitializer), typeof(StoreDbInitializer));

            services.AddScoped(typeof(AuditInterceptor));



            #endregion


            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork.UnitOfWork));

            services.AddScoped(typeof(UserManager<ApplicationUser>));
           

            #region IdentityDbContext
            services.AddDbContext<StoreIdentityDbContext>(optionsBuilder =>
               {
                   optionsBuilder.UseLazyLoadingProxies();
                   optionsBuilder.UseSqlServer(configuration.GetConnectionString("IdentityContext"));
               });

            services.AddScoped(typeof(IStoreIdentityDbInitializer), typeof(StoreIdentityDbInitializer));

            #endregion

            return services;
        }
    }
}
