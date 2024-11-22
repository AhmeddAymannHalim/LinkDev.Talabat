

namespace LinkDev.Talabat.Core.Domain.Specifications.Order

{
    public class OrderSpecifications : BaseSpecifications<Entities.Orders.Order, int>
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
