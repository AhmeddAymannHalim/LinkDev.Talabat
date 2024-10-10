using LinkDev.Talabat.Core.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Domain.Specifications.Products
{
    public class ProductWithBrandAndCategorySpecifications : BaseSpecifications<Product,int>
    {
        //This Object is Created via The consturctor will be use for building the Query that get All Products
        public ProductWithBrandAndCategorySpecifications():base()
        {
            AddIncludes();
        }

       

        //This Object is Created via The consturctor will be use for building the Query that get SpecificProduct

        public ProductWithBrandAndCategorySpecifications(int id) : base(id)
        {
            AddIncludes();
        }

        private void AddIncludes()
        {
            Includes.Add(P => P.Brand!);
            Includes.Add(P => P.Category!);
        }
    }
}
