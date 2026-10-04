using AutoMapper;
using LinkDev.Talabat.Core.Application;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Products;
using LinkDev.Talabat.Core.Application.Services.Products;
using LinkDev.Talabat.Core.Domain.Contracts;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using Moq;

namespace LinkDev.Talabat.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IGenericRepository<Product, int>> _products = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly ProductService _sut;

        public ProductServiceTests()
        {
            _unitOfWork.Setup(u => u.GetRepository<Product, int>()).Returns(_products.Object);
            _sut = new ProductService(_unitOfWork.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetProductAsync_WhenMissing_ThrowsNotFound()
        {
            _products.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecfifcations<Product, int>>()))
                     .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetProductAsync(404));
        }

        [Fact]
        public async Task GetProductsAsync_ReturnsPageInfoAndCountFromRepository()
        {
            _products.Setup(r => r.GetAllWithSpecAsync(It.IsAny<ISpecfifcations<Product, int>>(), false))
                     .ReturnsAsync(new List<Product>());
            _products.Setup(r => r.GetCountAsync(It.IsAny<ISpecfifcations<Product, int>>())).ReturnsAsync(18);
            _mapper.Setup(m => m.Map<IEnumerable<ProductToReturnDto>>(It.IsAny<object>()))
                   .Returns(new List<ProductToReturnDto>());

            var result = await _sut.GetProductsAsync(new ProductSpecParams { PageIndex = 2, PageSize = 5 });

            Assert.Equal(2, result.PageIndex);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(18, result.Count);
        }
    }

    public class ProductSpecParamsTests
    {
        [Theory]
        [InlineData(50, 10)]   // capped at the max page size
        [InlineData(0, 1)]     // never below one
        [InlineData(-3, 1)]
        [InlineData(7, 7)]
        public void PageSize_IsClampedToValidRange(int input, int expected) =>
            Assert.Equal(expected, new ProductSpecParams { PageSize = input }.PageSize);

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-5, 1)]
        [InlineData(3, 3)]
        public void PageIndex_NeverBelowOne(int input, int expected) =>
            Assert.Equal(expected, new ProductSpecParams { PageIndex = input }.PageIndex);

        [Fact]
        public void Search_IsUpperCased() =>
            Assert.Equal("PIZZA", new ProductSpecParams { Search = "pizza" }.Search);
    }
}
