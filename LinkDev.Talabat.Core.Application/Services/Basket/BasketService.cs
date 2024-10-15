using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Basket;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Basket;
using LinkDev.Talabat.Core.Domain.Contracts.Infrastructure;
using LinkDev.Talabat.Core.Domain.Entities.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Services.Basket
{
    public class BasketService(IBasketRepository basketRepository,IMapper mapper) : IBasketService
    {
        public async Task<CustomerBasketDto> GetCustomerBasketAsync(string basketId)
        {
            var basket = await basketRepository.GetBasketAsync(basketId);

            if(basketId is null)
            {
                return new CustomerBasketDto() {Id = basketId! };
            }

            return  mapper.Map<CustomerBasketDto>(basket);
           
        }



        public async Task<CustomerBasketDto>? UpdateCustomerBasketAsync(CustomerBasketDto basketDto , TimeSpan timeToLive)
        {
            var basket = mapper.Map<CustomerBasket>(basketDto);
            var updatedBasket = await basketRepository.UpdateBasketAsync(basket , timeToLive);

            if (updatedBasket is null) throw new Exception();

            return basketDto;

          
        }

        public async Task DeleteCustomerBasket(string id)
        {
           var isDeleted = await basketRepository.DeleteBasketAsync(id);

            if (!isDeleted)
                throw new Exception();
        }

    }
}
