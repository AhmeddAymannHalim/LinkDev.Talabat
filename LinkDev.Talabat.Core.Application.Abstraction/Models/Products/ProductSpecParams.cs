using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Abstraction.Models.Products
{
    public class ProductSpecParams
    {
        public string? sort { get; set; }

        private string? search;

        public string? Search

        {
            get { return search; }
            set { search = value?.ToUpper(); }
        }


        public int? BrandId { get; set; }

        public int? CategoryId { get; set; }

        private const int maxPageSize = 10;

        private int pageSize = 5;

        public int PageIndex { get; set; } = 1;

        public int PageSize
        {
            get
            {
                return pageSize;
            }
            set
            {
                pageSize = value > maxPageSize ? maxPageSize : value;
            }

        }
    }
}
