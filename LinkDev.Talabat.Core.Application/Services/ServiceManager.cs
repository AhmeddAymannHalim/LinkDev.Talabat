using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Services;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Auth;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Basket;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Employees;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Orders;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Products;
using LinkDev.Talabat.Core.Application.Services.Auth;
using LinkDev.Talabat.Core.Application.Services.Basket;
using LinkDev.Talabat.Core.Application.Services.Employees;
using LinkDev.Talabat.Core.Application.Services.Products;
using LinkDev.Talabat.Core.Domain.Contracts.Infrastructure;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkDev.Talabat.Core.Application
{
    internal class ServiceManager : IServiceManager
    {
        private readonly Lazy<IOrderService> _orderService;
        private readonly Lazy<IProductService> _productService;
        private readonly Lazy<IEmployeeService> _employeeService;
        private readonly Lazy<IBasketService> _basketService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly Lazy<IAuthService> _authService;

      

        public ServiceManager
            (
            IUnitOfWork unitOfWork,
            IMapper mapper,
            Func<IBasketService> basketServiceFactory,
            IConfiguration configuration,
            Func<IAuthService> authServiceFactory,
            Func<IOrderService> orderServiceFactory
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _productService = new Lazy<IProductService>(() => new ProductService(unitOfWork, mapper));
            _employeeService = new Lazy<IEmployeeService>(() => new EmployeeService(unitOfWork, mapper));
            _basketService = new Lazy<IBasketService>(basketServiceFactory,LazyThreadSafetyMode.ExecutionAndPublication);
            _authService = new Lazy<IAuthService>(authServiceFactory, LazyThreadSafetyMode.ExecutionAndPublication);
            _orderService = new Lazy<IOrderService>(orderServiceFactory,LazyThreadSafetyMode.ExecutionAndPublication);
            

        }

        public IOrderService OrderService => _orderService.Value;

        public IProductService ProductService => _productService.Value;

        public IEmployeeService EmployeeService => _employeeService.Value;

        public IBasketService BasketService => _basketService.Value;

        public IAuthService AuthService => _authService.Value;

    }
}
