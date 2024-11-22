using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Orders;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Basket;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Orders;
using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Entities.Orders;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using LinkDev.Talabat.Core.Domain.Specifications.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Services.Orders
{
    internal class OrderService(IBasketService basketService, IMapper mapper, IUnitOfWork unitOfWork) : IOrderService
    {
        public async Task<OrderToReturnDto> CreateOrderAsync(string buyeremail, OrderToCreateDto order)
        {
           
            // 1.Get Basket From Baskets Repository

            var basket = await basketService.GetCustomerBasketAsync(order.BasketId);


            // 2.Get Selected Items at Basket From Products Repoistory


            var orderItems = new List<OrderItem>();

            if(basket.Items.Count > 0)
            {
                var productRepo = unitOfWork.GetRepository<Product, int>();
                foreach (var item in basket.Items)
                {
                    var product = await productRepo.GetAsync(item.Id);

                    if(product is not null)
                    {
                        var productItemOrderd = new ProductItemOrderd()
                        {
                            ProductItemOrderdId = product.Id,
                            ProductName = product.Name,
                            PictureUrl = product.PictureUrl ?? "",
                        };

                        var orderItem = new OrderItem()
                        {
                            Product = productItemOrderd,
                            Price = product.Price,
                            Quantity = item.Quantity,

                        };
                        orderItems.Add(orderItem);

                    }


                }
            }


            // 3.Calculate SubTotal

            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);


            //4. Mapping
            var address = mapper.Map<Address>(order.ShippingAddress);

            //Get Delivery Method

            var deliveryMethod = await unitOfWork.GetRepository<DeliveryMethod, int>().GetAsync(order.DeliveryMethodId);


            // 5.Create Order

            var orderToCreate = new Order()
            {
                BuyerEmail = buyeremail,
                ShippingAddress = address,
                DeliveryMethod = deliveryMethod,
                Items = orderItems,
                SubTotal = subTotal,
            };


            await unitOfWork.GetRepository<Order, int>().AddAsync(orderToCreate);

            // 6.Save To Database

             var created = await unitOfWork.CompleteAsync() > 0;

            if (!created) throw new BadRequestException("an error has occured during creating the order");

            return mapper.Map<OrderToReturnDto>(orderToCreate);

        
        }

        public async Task<IEnumerable<OrderToReturnDto>> GetOrdersForUserAsync(string buyerEmail)
        {

            var orderSpecs = new OrderSpecifications(buyerEmail);
             
            var orders = await unitOfWork.GetRepository<Order,int>().GetAllWithSpecAsync(orderSpecs); 


            return mapper.Map<IEnumerable<OrderToReturnDto>>(orders);
        }

        public async Task<OrderToReturnDto> GetOrderByIdAsync(string buyerEmail, int orderId)
        {
            var orderSpecs = new OrderSpecifications(buyerEmail,orderId);

            var order = await unitOfWork.GetRepository<Order, int>().GetWithSpecAsync(orderSpecs);

            if (order is null) throw new NotFoundException(nameof(order),orderId);

            return mapper.Map<OrderToReturnDto>(order); 
        }

        public async Task<IEnumerable<DeliveryMethodDto>> GetDeliveryMethodsAsync()
        {
            var deliveryMethod = await unitOfWork.GetRepository<DeliveryMethod, int>().GetAllAsync();


            return mapper.Map<IEnumerable<DeliveryMethodDto>>(deliveryMethod);


        }

    }
}
