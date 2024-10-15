using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Abstraction.Models.Basket
{
    public class CustomerBasketDto 
    {
        public required string Id { get; set; }

        public List<BasketItemDto> Items { get; set; } = new List<BasketItemDto>(); 
        // Count = 0 On IconChart -- Or you can make it nullable and it will not show count above charticon....
    }
}
