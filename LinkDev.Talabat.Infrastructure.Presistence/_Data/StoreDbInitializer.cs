using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence.DbInitializers;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using LinkDev.Talabat.Infrastructure.Presistence._Common;
using System.Text.Json;

namespace LinkDev.Talabat.Infrastructure.Presistence.Data
{
    internal sealed class StoreDbInitializer(StoreDbContext _dbContext) : DbInitializer(_dbContext), IStoreDbInitializer
    {
       

        

        public override async Task SeedAsync()
        {
            #region Brand
            if (!_dbContext.Brands.Any())
            {
                var brandsData = await File.ReadAllTextAsync($"../LinkDev.Talabat.Infrastructure.Presistence/Data/Seeds/brands.json");

                var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);

                if (brands?.Count > 0)

                    await _dbContext.Brands.AddRangeAsync(brands);
                await _dbContext.SaveChangesAsync();




            } 
            #endregion

            #region Category
            if (!_dbContext.Categories.Any())
            {
                var categoriesData = await File.ReadAllTextAsync($"../LinkDev.Talabat.Infrastructure.Presistence/Data/Seeds/categories.json");

                var categories = JsonSerializer.Deserialize<List<ProductCategory>>(categoriesData);

                if (categories?.Count > 0)

                    await _dbContext.Set<ProductCategory>().AddRangeAsync(categories);
                await _dbContext.SaveChangesAsync();
            }
            #endregion

            #region Product
            if (!_dbContext.Products.Any())
            {
                var productsData = await File.ReadAllTextAsync($"../LinkDev.Talabat.Infrastructure.Presistence/Data/Seeds/products.json");

                var products = JsonSerializer.Deserialize<List<Product>>(productsData);

                if (products?.Count > 0)

                    await _dbContext.Set<Product>().AddRangeAsync(products);
                await _dbContext.SaveChangesAsync();
            }
            #endregion


        }
    }
}
