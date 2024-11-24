using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models._Common;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Basket;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Employees;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Orders;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Products;

using LinkDev.Talabat.Core.Domain.Entities.Basket;
using LinkDev.Talabat.Core.Domain.Entities.Employees;
using LinkDev.Talabat.Core.Domain.Entities.Orders;
using LinkDev.Talabat.Core.Domain.Entities.Products;

using UserAddress = LinkDev.Talabat.Core.Domain.Entities.Orders.Address;
using OrderAddress = LinkDev.Talabat.Core.Domain.Entities._Identity.Address;
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

            CreateMap<OrderTable, OrderToReturnDto>()
                .ForMember(dist => dist.DeliveryMethod,options =>
                {
                    options.MapFrom(src => src.DeliveryMethod!.ShortName);
                });

            CreateMap<OrderItem,OrderItemDto>()
                .ForMember(dist => dist.ProductId,options => options.MapFrom(src =>src.Product.ProductId))
                .ForMember(dist => dist.ProductName,options => options.MapFrom(src =>src.Product.ProductName))
                .ForMember(dist => dist.PictureUrl,options => options.MapFrom<OrderItemPictureUrlResolver>());

            CreateMap<OrderAddress, AddressDto>().ReverseMap();


            CreateMap<DeliveryMethod, DeliveryMethodDto>();

            CreateMap<UserAddress, AddressDto>().ReverseMap();
        }
    }
}
