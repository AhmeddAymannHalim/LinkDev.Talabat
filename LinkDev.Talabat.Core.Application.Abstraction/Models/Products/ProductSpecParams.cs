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

        private int pageIndex = 1;
        public int PageIndex
        {
            get { return pageIndex; }
            set { pageIndex = Math.Max(value, 1); }
        }

        public int PageSize
        {
            get
            {
                return pageSize;
            }
            set
            {
                pageSize = Math.Clamp(value, 1, maxPageSize);
            }

        }
    }
}
