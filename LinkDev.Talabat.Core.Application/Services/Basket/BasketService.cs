using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Basket;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Basket;
using LinkDev.Talabat.Core.Domain.Contracts.Infrastructure;
using LinkDev.Talabat.Core.Domain.Entities.Basket;
using Microsoft.Extensions.Configuration;

namespace LinkDev.Talabat.Core.Application.Services.Basket
{
    public class BasketService(IBasketRepository basketRepository,IMapper mapper,IConfiguration configuration) : IBasketService
    {
        public async Task<CustomerBasketDto> GetCustomerBasketAsync(string basketId)
        {
            var basket = await basketRepository.GetBasketAsync(basketId);

            if (basketId is null) throw new NotFoundException(nameof(CustomerBasketDto),basketId!);

            return  mapper.Map<CustomerBasketDto>(basket);
           
        }

        public async Task<CustomerBasketDto>? UpdateCustomerBasketAsync(CustomerBasketDto basketDto )
        {
            var basket = mapper.Map<CustomerBasket>(basketDto);
            var timeToLive = TimeSpan.FromDays(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInDays"]!));
            var updatedBasket = await basketRepository.UpdateBasketAsync(basket , timeToLive);

            if (updatedBasket is null) throw new BadRequestException("can't update,there is a problem with this basket.");

            return basketDto;

          
        }

        public async Task DeleteCustomerBasket(string id)
        {
           var deleted = await basketRepository.DeleteBasketAsync(id);

            if (!deleted)
                throw new BadRequestException("unable to delete this basket.");
        }

    }
}
