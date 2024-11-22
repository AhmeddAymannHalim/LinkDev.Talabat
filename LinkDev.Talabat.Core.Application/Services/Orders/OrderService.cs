﻿using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Orders;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Basket;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Orders;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Entities.Orders;
using LinkDev.Talabat.Core.Domain.Entities.Products;
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

            // 5.Create Order

            var orderToCreate = new Order()
            {
                BuyerEmail = buyeremail,
                ShippingAddress = address,
                DeliveryMethodId = order.DeliveryMethodId,
                Items = orderItems,
                SubTotal = subTotal,
            };


            await unitOfWork.GetRepository<Order, int>().AddAsync(orderToCreate);

            // 6.Save To Database

             var created = await unitOfWork.CompleteAsync() > 0;

            if (!created) throw new BadRequestException("an error has occured during creating the order");

            return mapper.Map<OrderToReturnDto>(orderToCreate);

        
        }

        public Task<IEnumerable<DeliveryMethodDto>> GetDeliveryMethodsAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<OrderToReturnDto> GetOrderByIdAsync(string email, int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrderToReturnDto>> GetOrdersForUserAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
