using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Domain.Entities.Orders
{
    public class Order :BaseAuditableEntity<int>
    {

        public required string BuyerEmail { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public virtual required Address ShippingAddress { get; set; } //Navigation Property For OwnEntity

        public int? DeliveryMethodId { get; set; }

        public virtual DeliveryMethod? DeliveryMethod{ get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

        public decimal SubTotal { get; set; }

        //[NotMapped]
        //public decimal Total => SubTotal + DeliveryMethod!.Cost;

        public decimal GetTotal() => SubTotal + DeliveryMethod!.Cost;

        public string PaymentIntentId { get; set; } = "";
    }

}
