using AutoMapper;
using LinkDev.Talabat.Core.Application;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Basket;
using LinkDev.Talabat.Core.Application.Services.Basket;
using LinkDev.Talabat.Core.Domain.Contracts.Infrastructure;
using LinkDev.Talabat.Core.Domain.Entities.Basket;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LinkDev.Talabat.Tests
{
    public class BasketServiceTests
    {
        private readonly Mock<IBasketRepository> _repo = new();
        private readonly BasketService _sut;

        public BasketServiceTests()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CustomerBasket, CustomerBasketDto>().ReverseMap();
                cfg.CreateMap<BasketItem, BasketItemDto>().ReverseMap();
            }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance).CreateMapper();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["RedisSettings:TimeToLiveInDays"] = "16" })
                .Build();

            _sut = new BasketService(_repo.Object, mapper, config);
        }

        [Fact]
        public async Task GetCustomerBasketAsync_WhenBasketMissing_ThrowsNotFound()
        {
            _repo.Setup(r => r.GetAsync("missing")).ReturnsAsync((CustomerBasket?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetCustomerBasketAsync("missing"));
        }

        [Fact]
        public async Task GetCustomerBasketAsync_WhenBasketExists_ReturnsMappedDto()
        {
            _repo.Setup(r => r.GetAsync("b1")).ReturnsAsync(new CustomerBasket { Id = "b1" });

            var result = await _sut.GetCustomerBasketAsync("b1");

            Assert.Equal("b1", result.Id);
        }

        [Fact]
        public async Task UpdateCustomerBasketAsync_WhenRepositoryReturnsNull_ThrowsBadRequest()
        {
            _repo.Setup(r => r.UpdateAsync(It.IsAny<CustomerBasket>(), It.IsAny<TimeSpan>()))
                 .ReturnsAsync((CustomerBasket?)null);

            await Assert.ThrowsAsync<BadRequestException>(
                () => _sut.UpdateCustomerBasketAsync(new CustomerBasketDto { Id = "b1" })!);
        }

        [Fact]
        public async Task UpdateCustomerBasketAsync_UsesConfiguredTimeToLive()
        {
            _repo.Setup(r => r.UpdateAsync(It.IsAny<CustomerBasket>(), It.IsAny<TimeSpan>()))
                 .ReturnsAsync(new CustomerBasket { Id = "b1" });

            await _sut.UpdateCustomerBasketAsync(new CustomerBasketDto { Id = "b1" })!;

            _repo.Verify(r => r.UpdateAsync(It.IsAny<CustomerBasket>(), TimeSpan.FromDays(16)), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomerBasket_WhenRepositoryFails_ThrowsBadRequest()
        {
            _repo.Setup(r => r.DeleteAsync("b1")).ReturnsAsync(false);

            await Assert.ThrowsAsync<BadRequestException>(() => _sut.DeleteCustomerBasket("b1"));
        }
    }
}
