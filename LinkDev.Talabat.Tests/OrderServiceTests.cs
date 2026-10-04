using AutoMapper;
using LinkDev.Talabat.Core.Application;
using LinkDev.Talabat.Core.Application.Abstraction.Models._Common;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Basket;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Orders;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Basket;
using LinkDev.Talabat.Core.Application.Services.Orders;
using LinkDev.Talabat.Core.Domain.Contracts;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Entities.Orders;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using Moq;

namespace LinkDev.Talabat.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IBasketService> _basketService = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IGenericRepository<Product, int>> _products = new();
        private readonly Mock<IGenericRepository<DeliveryMethod, int>> _deliveryMethods = new();
        private readonly Mock<IGenericRepository<OrderTable, int>> _orders = new();
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _unitOfWork.Setup(u => u.GetRepository<Product, int>()).Returns(_products.Object);
            _unitOfWork.Setup(u => u.GetRepository<DeliveryMethod, int>()).Returns(_deliveryMethods.Object);
            _unitOfWork.Setup(u => u.GetRepository<OrderTable, int>()).Returns(_orders.Object);
            _unitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            _mapper.Setup(m => m.Map<Address>(It.IsAny<AddressDto>()))
                   .Returns(new Address { FirstName = "A", LastName = "B", Street = "S", City = "C", Country = "EG" });

            _sut = new OrderService(_basketService.Object, _mapper.Object, _unitOfWork.Object);
        }

        private static OrderToCreateDto NewOrder(int deliveryMethodId = 1) => new()
        {
            BasketId = "b1",
            DeliveryMethodId = deliveryMethodId,
            ShippingAddress = new AddressDto
            {
                FirstName = "A", LastName = "B", Street = "S", City = "C", Country = "EG"
            },
        };

        private void SetupBasket(params (int productId, int quantity)[] items) =>
            _basketService.Setup(b => b.GetCustomerBasketAsync("b1")).ReturnsAsync(new CustomerBasketDto
            {
                Id = "b1",
                Items = items.Select(i => new BasketItemDto
                {
                    Id = i.productId, ProductName = "x", Price = 1, Quantity = i.quantity
                }).ToList(),
            });

        private static Product NewProduct(int id, decimal price) =>
            new() { Id = id, Name = $"P{id}", NormalizedName = $"P{id}", Description = "d", Price = price };

        private static DeliveryMethod NewDelivery(decimal cost) =>
            new() { Id = 1, ShortName = "Fast", Description = "d", DeliveryTime = "1d", Cost = cost };

        [Fact]
        public async Task CreateOrderAsync_EmptyBasket_ThrowsBadRequest()
        {
            SetupBasket();

            await Assert.ThrowsAsync<BadRequestException>(() => _sut.CreateOrderAsync("a@a.com", NewOrder()));
            _orders.Verify(r => r.AddAsync(It.IsAny<OrderTable>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_UnknownDeliveryMethod_ThrowsNotFound()
        {
            SetupBasket((1, 1));
            _deliveryMethods.Setup(r => r.GetAsync(99)).ReturnsAsync((DeliveryMethod?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateOrderAsync("a@a.com", NewOrder(99)));
            _orders.Verify(r => r.AddAsync(It.IsAny<OrderTable>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_UnknownProduct_ThrowsNotFound()
        {
            SetupBasket((7, 1));
            _deliveryMethods.Setup(r => r.GetAsync(1)).ReturnsAsync(NewDelivery(5));
            _products.Setup(r => r.GetAsync(7)).ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateOrderAsync("a@a.com", NewOrder()));
            _orders.Verify(r => r.AddAsync(It.IsAny<OrderTable>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidBasket_UsesDatabasePricesAndComputesTotals()
        {
            SetupBasket((1, 2), (2, 3));
            _deliveryMethods.Setup(r => r.GetAsync(1)).ReturnsAsync(NewDelivery(5));
            _products.Setup(r => r.GetAsync(1)).ReturnsAsync(NewProduct(1, 10m));
            _products.Setup(r => r.GetAsync(2)).ReturnsAsync(NewProduct(2, 4m));

            OrderTable? saved = null;
            _orders.Setup(r => r.AddAsync(It.IsAny<OrderTable>()))
                   .Callback<OrderTable>(o => saved = o)
                   .Returns(Task.CompletedTask);

            await _sut.CreateOrderAsync("a@a.com", NewOrder());

            Assert.NotNull(saved);
            Assert.Equal("a@a.com", saved!.BuyerEmail);
            Assert.Equal(2, saved.Items.Count);
            Assert.Equal(2 * 10m + 3 * 4m, saved.SubTotal);   // 32
            Assert.Equal(37m, saved.GetTotal());              // 32 + delivery 5
            _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenNothingSaved_ThrowsBadRequest()
        {
            SetupBasket((1, 1));
            _deliveryMethods.Setup(r => r.GetAsync(1)).ReturnsAsync(NewDelivery(5));
            _products.Setup(r => r.GetAsync(1)).ReturnsAsync(NewProduct(1, 10m));
            _unitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            await Assert.ThrowsAsync<BadRequestException>(() => _sut.CreateOrderAsync("a@a.com", NewOrder()));
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenNotFound_ThrowsNotFound()
        {
            _orders.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecfifcations<OrderTable, int>>()))
                   .ReturnsAsync((OrderTable?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetOrderByIdAsync("a@a.com", 1));
        }
    }
}
