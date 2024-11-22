using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Domain.Entities.Orders
{
    public class ProductItemOrderd
    {
        
        public int ProductItemOrderdId { get; set; }

        public required string ProductName { get; set; }

        public required string PictureUrl { get; set; }
    }
}
