using LinkDev.Talabat.Core.Domain.Entities.Orders;

namespace LinkDev.Talabat.Core.Domain.Specifications.Orders

{
    public class OrderSpecifications : BaseSpecifications<OrderTable,int>
    {
        public OrderSpecifications(string buyerEmail):base(O => O.BuyerEmail == buyerEmail)
        {
            AddIncludes();
            AddOrderByDesc(order => order.OrderDate);
        }

        public OrderSpecifications(string buyerEmail, int orderId) : base(O => O.Id == orderId && O.BuyerEmail == buyerEmail)
        {
            AddIncludes();
        }

        private protected override void AddIncludes()
        {
            base.AddIncludes();
            Includes.Add(order => order.DeliveryMethod!);
            Includes.Add(order => order.Items);
        }
    }
}
