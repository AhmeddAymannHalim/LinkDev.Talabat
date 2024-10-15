using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Basket;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Employees;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Products;
using LinkDev.Talabat.Core.Domain.Entities.Basket;
using LinkDev.Talabat.Core.Domain.Entities.Employees;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Mapping
{
    internal class MappingProfile : Profile
    {
        

        public MappingProfile()
        {
            CreateMap<Product, ProductToReturnDto>()
                    .ForMember(D => D.Brand, O => O.MapFrom(P => P.Brand!.Name))
                    .ForMember(D => D.Category, O => O.MapFrom(P => P.Category!.Name))
                    .ForMember(D => D.PictureUrl,X => X.MapFrom<ProductPictureUrlResolver>());
                     
            CreateMap<ProductBrand, BrandDto>();
            CreateMap<ProductCategory, CategoryDto>();
            CreateMap<Employee, EmployeeToReturnDto>();
            CreateMap<CustomerBasket, CustomerBasketDto>().ReverseMap();
            CreateMap<BasketItem, BasketItemDto>().ReverseMap();
        }
    }
}
