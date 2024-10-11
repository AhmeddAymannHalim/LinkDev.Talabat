using LinkDev.Talabat.Core.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Domain.Specifications.Product_Specs
{
    public class ProductWithBrandAndCategorySpecifications : BaseSpecifications<Product,int>
    {
        //This Object is Created via The consturctor will be use for building the Query that get All Products
        public ProductWithBrandAndCategorySpecifications(string? sort, int? brandId, int? categoryId, int pageSize, int pageIndex,string? search) : base
            (
             P =>
                (string.IsNullOrEmpty(search) || P.NormalizedName.Contains(search))

                                         && 

                (!brandId.HasValue || P.BrandId == brandId.Value)

                                         &&

                (!categoryId.HasValue || P.CategoryId == categoryId.Value)
                
             )
            
        {

            AddIncludes();

            switch (sort)
            {
                case "priceAsc":
                    AddOrderBy(P => P.Price);
                    break;
                case "priceDesc":
                    AddOrderByDesc(P => P.Price);
                    break;
                default:
                    AddOrderBy(P => P.Name);
                    break;
                 
              
            }
            //totalProducts = 18 ,
            //PageSize      = 5  ,
            //PageIndex     = 3  ,

            ApplyPagination(pageSize *(pageIndex -1),pageSize);
        }

        //This Object is Created via The consturctor will be use for building the Query that get SpecificProduct

        public ProductWithBrandAndCategorySpecifications(int id) : base(id)
        {
            AddIncludes();
        }

        private protected override void AddIncludes()
        {
            base.AddIncludes();
            Includes.Add(P => P.Brand!);
            Includes.Add(P => P.Category!);
        }
    }
}
