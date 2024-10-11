using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Services;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Employees;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Products;
using LinkDev.Talabat.Core.Application.Services.Employees;
using LinkDev.Talabat.Core.Application.Services.Products;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application
{
    internal class ServiceManager : IServiceManager
    {
        private readonly Lazy<IProductService> _productService;
        private readonly Lazy<IEmployeeService> _employeeService;

        public ServiceManager(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _productService = new Lazy<IProductService>(() => new ProductService(unitOfWork, mapper));
            _employeeService = new Lazy<IEmployeeService>(() => new EmployeeService(unitOfWork, mapper));
        }

        public IProductService ProductService => _productService.Value;

        public IEmployeeService EmployeeService => _employeeService.Value;
    }
}
