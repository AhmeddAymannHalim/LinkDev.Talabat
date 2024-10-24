using LinkDev.Talabat.Core.Domain.Entities._Identity;
using LinkDev.Talabat.Infrastructure.Presistence._Identity.Config;
using LinkDev.Talabat.Infrastructure.Presistence.Identity.Config;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LinkDev.Talabat.Infrastructure.Presistence._Identity
{
    public class StoreIdentityDbContext(DbContextOptions<StoreIdentityDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.ApplyConfiguration(new ApplicationUserConfigurations());
            builder.ApplyConfiguration(new AddressConfigurations());

            #region ApplyConfiguration - OldCode

            //    var assembly = typeof(AssemblyInformation).Assembly;

            //    var configurations = assembly.GetTypes()
            //                         .Where(T => T.GetInterfaces()
            //                         .Any(I => I.IsGenericType &&
            //                                     I.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)&&
            //                                     I.BaseType != (typeof(BaseEntityConfigurations<,>))
            //                                     ))
            //                         .ToList();

            //    foreach (var configuration in configurations) 
            //    {
            //          var instance = Activator.CreateInstance(configuration);
            //            builder.ApplyConfiguration((dynamic) instance!);

            //    }


            #endregion

            //public DbSet<Address> Addresses { get; set; }
        }
    }
}
