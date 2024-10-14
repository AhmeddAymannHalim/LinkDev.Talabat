using LinkDev.Talabat.APIs.Controllers.Controllers.Base;
using LinkDev.Talabat.APIs.Controllers.Errors;
using LinkDev.Talabat.Core.Application.Abstraction.Common;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Products;
using LinkDev.Talabat.Core.Application.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.APIs.Controllers.Controllers.Products
{
    public class ProductsController(IServiceManager serviceManager) : ApiControllerBase
    {
        [HttpGet]
        //Get: /api/Products
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery]ProductSpecParams specParams)
        {
            var products = await serviceManager.ProductService.GetProductsAsync(specParams);
            return Ok(products);
        }

        [HttpGet("{id}")] //Get: /api/Products/?{id}
        //Get: /api/Product/{id}
        public async Task<ActionResult<IEnumerable<ProductToReturnDto>>> GetProduct(int id)
        {
            var product = await serviceManager.ProductService.GetProductAsync(id);

            if (product is null)
                return NotFound(new ApiResponse(404,$"The Product With Id {id} is Not Found"));
            return Ok(product);
        }

        [HttpGet("brands")] //Get : api/Products/brands
        public async Task<ActionResult<IEnumerator<BrandDto>>> GetBrands()
        {
            var brands = await serviceManager.ProductService.GetBrandsAsync();

            return Ok(brands);
        }

        [HttpGet("categories")] //Get : api/Products/categories
        public async Task<ActionResult<IEnumerator<CategoryDto>>> GetCategories()
        {
            var categories = await serviceManager.ProductService.GetCategoriesAsync();

            return Ok(categories);
        }


    }
}
