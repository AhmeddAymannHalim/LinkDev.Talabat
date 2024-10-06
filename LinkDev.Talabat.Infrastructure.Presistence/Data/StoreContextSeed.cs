using LinkDev.Talabat.Core.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Infrastructure.Presistence.Data
{
    public static class StoreContextSeed
    {
        public static async Task SeedAsync(StoreContext dbContext)
        {
            if (!dbContext.Brands.Any())
            {


                #region Brand
                var brandsData = await File.ReadAllTextAsync($"../LinkDev.Talabat.Infrastructure.Presistence/Data/Seeds/brands.json");

                var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);

                if (brands?.Count > 0)

                    await dbContext.Brands.AddRangeAsync(brands);
                    await dbContext.SaveChangesAsync();

                #endregion

               

            }
            if (!dbContext.Categories.Any())
            {
                var categoriesData = await File.ReadAllTextAsync($"../LinkDev.Talabat.Infrastructure.Presistence/Data/Seeds/categories.json");

                var categories = JsonSerializer.Deserialize<List<ProductCategory>>(categoriesData);

                if (categories?.Count > 0)

                    await dbContext.Set<ProductCategory>().AddRangeAsync(categories);
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.Products.Any())
            {
                var productsData = await File.ReadAllTextAsync($"../LinkDev.Talabat.Infrastructure.Presistence/Data/Seeds/products.json");

                var products = JsonSerializer.Deserialize<List<Product>>(productsData);

                if (products?.Count > 0)

                    await dbContext.Set<Product>().AddRangeAsync(products);
                    await dbContext.SaveChangesAsync();
            }
        }
    }
}
