using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Domain.Entities.Basket
{
    public class CustomerBasket :BaseEntity<string>
    {
         public ICollection<BasketItem> Items { get; set; } = new HashSet<BasketItem>();

    }
}
