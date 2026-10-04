using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Orders;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Basket;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Orders;
using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Entities.Orders;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using LinkDev.Talabat.Core.Domain.Specifications.Orders;
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
            var basket = await basketService.GetCustomerBasketAsync(order.BasketId);

            if (basket.Items.Count == 0)
                throw new BadRequestException("Can't create an order from an empty basket.");

            var deliveryMethod = await unitOfWork.GetRepository<DeliveryMethod, int>().GetAsync(order.DeliveryMethodId)
                ?? throw new NotFoundException(nameof(DeliveryMethod), order.DeliveryMethodId);

            // Prices come from the database, never from the client's basket.
            var productRepo = unitOfWork.GetRepository<Product, int>();
            var orderItems = new List<OrderItem>();

            foreach (var item in basket.Items)
            {
                var product = await productRepo.GetAsync(item.Id)
                    ?? throw new NotFoundException(nameof(Product), item.Id);

                orderItems.Add(new OrderItem()
                {
                    Product = new ProductItemOrderd()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        PictureUrl = product.PictureUrl ?? "",
                    },
                    Price = product.Price,
                    Quantity = item.Quantity,
                });
            }

            var orderToCreate = new OrderTable()
            {
                BuyerEmail = buyeremail,
                ShippingAddress = mapper.Map<Address>(order.ShippingAddress),
                DeliveryMethod = deliveryMethod,
                Items = orderItems,
                SubTotal = orderItems.Sum(item => item.Price * item.Quantity),
            };


            await unitOfWork.GetRepository<OrderTable, int>().AddAsync(orderToCreate);

            // 6.Save To Database

             var created = await unitOfWork.CompleteAsync() > 0;

            if (!created) throw new BadRequestException("an error has occured during creating the order");

            return mapper.Map<OrderToReturnDto>(orderToCreate);

        
        }

        public async Task<IEnumerable<OrderToReturnDto>> GetOrdersForUserAsync(string buyerEmail)
        {

            var orderSpecs = new OrderSpecifications(buyerEmail);
             
            var orders = await unitOfWork.GetRepository<OrderTable,int>().GetAllWithSpecAsync(orderSpecs); 


            return mapper.Map<IEnumerable<OrderToReturnDto>>(orders);
        }

        public async Task<OrderToReturnDto> GetOrderByIdAsync(string buyerEmail, int orderId)
        {
            var orderSpecs = new OrderSpecifications(buyerEmail,orderId);

            var order = await unitOfWork.GetRepository<OrderTable, int>().GetWithSpecAsync(orderSpecs);

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
