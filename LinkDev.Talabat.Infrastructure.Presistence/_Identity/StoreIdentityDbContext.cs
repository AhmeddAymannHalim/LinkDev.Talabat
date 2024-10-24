using LinkDev.Talabat.Core.Domain.Entities._Identity;
using LinkDev.Talabat.Infrastructure.Presistence._Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;

namespace LinkDev.Talabat.Infrastructure.Presistence._Identity
{
    public class StoreIdentityDbContext(DbContextOptions<StoreIdentityDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            //builder.ApplyConfiguration(new ApplicationUserConfigurations());
            //builder.ApplyConfiguration(new AddressConfigurations());

            builder.ApplyConfigurationsFromAssembly(typeof(AssemblyInformation).Assembly,
                    type => type.GetCustomAttribute<DbContextTypeAttribute>()?.DbContextType == typeof(StoreIdentityDbContext));

            #region ApplyConfiguration - Using_Reflection

            //    var assembly = typeof(AssemblyInformation).Assembly;
               
            //    var configurations = assembly.GetTypes()
            //                                 .Where(T => T.GetInterfaces()
            //                                 .Any  (I => I.IsGenericType &&
            //                                             I.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)&&
            //                                             I.BaseType != (typeof(BaseEntityConfigurations<,>))
            //                                     ))
            //                         .ToList();

            //    foreach (var configuration in configurations) 
            //    {
            //          var instance = Activator.CreateInstance(configuration);
            //                         builder.ApplyConfiguration((dynamic) instance!);

            //    }


            #endregion

        }

    }
}
