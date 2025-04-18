﻿using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Core.Domain.Entities.Orders;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using LinkDev.Talabat.Infrastructure.Presistence._Common;
using LinkDev.Talabat.Infrastructure.Presistence._Data.Config.Orders;
using LinkDev.Talabat.Infrastructure.Presistence.Data.Config.Products;
using System.Numerics;
using System.Reflection;

namespace LinkDev.Talabat.Infrastructure.Presistence.Data
{
    public class StoreDbContext : DbContext
    {  
        
        public DbSet<Product> Products { get; set; }

        public DbSet<ProductBrand> Brands { get; set; }

        public DbSet<ProductCategory> Categories{ get; set; }

        public DbSet<OrderTable> Orders { get; set; }

        public DbSet<OrderItem> OrderItems{ get; set; }

        public DbSet<DeliveryMethod> DelivryMethods{ get; set; }

        public StoreDbContext(DbContextOptions<StoreDbContext> options):base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyInformation).Assembly,
                                   type => type.GetCustomAttribute<DbContextTypeAttribute>()?.DbContextType == typeof(StoreDbContext));

            modelBuilder.ApplyConfiguration(new OrderItemConfigurations());
            modelBuilder.ApplyConfiguration(new OrderConfigurations());
            modelBuilder.ApplyConfiguration(new DeliveryMethodConfigurations());
        }

      

      

    }
}
