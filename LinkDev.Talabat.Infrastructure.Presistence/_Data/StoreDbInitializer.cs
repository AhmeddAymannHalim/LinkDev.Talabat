using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence.DbInitializers;
using LinkDev.Talabat.Core.Domain.Entities.Orders;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using LinkDev.Talabat.Infrastructure.Presistence._Common;
using System.Text.Json;

namespace LinkDev.Talabat.Infrastructure.Presistence.Data
{
    internal sealed class StoreDbInitializer(StoreDbContext _dbContext) : DbInitializer(_dbContext), IStoreDbInitializer
    {
       

        

        // Seed files are copied to the output folder (see the .csproj), so this works from any working directory.
        private static string SeedFile(string name) => Path.Combine(AppContext.BaseDirectory, "_Data", "Seeds", name);

        public override async Task SeedAsync()
        {
            #region Brand
            if (!_dbContext.Brands.Any())
            {
                var brandsData = await File.ReadAllTextAsync(SeedFile("brands.json"));

                var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);

                if (brands?.Count > 0)

                    await _dbContext.Brands.AddRangeAsync(brands);
                await _dbContext.SaveChangesAsync();




            } 
            #endregion

            #region Category
            if (!_dbContext.Categories.Any())
            {
                var categoriesData = await File.ReadAllTextAsync(SeedFile("categories.json"));

                var categories = JsonSerializer.Deserialize<List<ProductCategory>>(categoriesData);

                if (categories?.Count > 0)

                    await _dbContext.Set<ProductCategory>().AddRangeAsync(categories);
                await _dbContext.SaveChangesAsync();
            }
            #endregion


            #region DeliveryMethod
            if (!_dbContext.DelivryMethods.Any())
            {
                var deliveryMethods = await File.ReadAllTextAsync(SeedFile("Delivery.json"));

                var deliveries = JsonSerializer.Deserialize<List<DeliveryMethod>>(deliveryMethods);

                if (deliveries?.Count > 0)

                    await _dbContext.Set<DeliveryMethod>().AddRangeAsync(deliveries);
                await _dbContext.SaveChangesAsync();
            }
            #endregion

            #region Product
            if (!_dbContext.Products.Any())
            {
                var productsData = await File.ReadAllTextAsync(SeedFile("products.json"));

                var products = JsonSerializer.Deserialize<List<Product>>(productsData);

                foreach (var product in products ?? [])
                    product.NormalizedName = product.Name.ToUpperInvariant();

                if (products?.Count > 0)

                    await _dbContext.Set<Product>().AddRangeAsync(products);
                await _dbContext.SaveChangesAsync();
            }
            #endregion



        }
    }
}
