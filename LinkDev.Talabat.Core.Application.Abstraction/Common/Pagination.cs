using LinkDev.Talabat.Core.Application.Abstraction.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Abstraction.Common
{
    public class Pagination<T>(int pageIndex, int pageSize, int count)
    {
        //private readonly ProductToReturnDto? productToReturnDto;

        public int PageIndex { get; set; } = pageIndex;

        public int PageSize { get; set; } = pageSize;

        public int Count { get; set; } = count;

        public required IEnumerable<T> Data { get; set; }

    }
}
