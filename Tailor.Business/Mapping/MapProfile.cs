using AutoMapper;
using Tailor.DTO.DTOs.AddressDtos;
using Tailor.DTO.DTOs.BlogCategoryDtos;
using Tailor.DTO.DTOs.BlogDtos;
using Tailor.DTO.DTOs.BlogDtos;
using Tailor.DTO.DTOs.CategoryDtos;
using Tailor.DTO.DTOs.ContactMessageDtos;
using Tailor.DTO.DTOs.CustomerSocialDtos;
using Tailor.DTO.DTOs.OrderDtos;

using Tailor.DTO.DTOs.ProductDtos;
using Tailor.DTO.DTOs.ProductPropertyDtos;
using Tailor.DTO.DTOs.ShoppingCartItem;
using Tailor.DTO.DTOs.StockDtos;
using Tailor.DTO.DTOs.SupportTicketDtos;
using Tailor.DTO.DTOs.CartDtos;
using Tailor.Entity.Entities;
using Tailor.Entity.Entities.Enums;

namespace Tailor.Business.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            // --- PRODUCT ---
            CreateMap<Product, CreateProductDto>().ReverseMap();
            CreateMap<Product, UpdateProductDto>().ReverseMap();
            CreateMap<Product, ResultProductDto>().ReverseMap();
            CreateMap<Product, ResultProductListDto>()
                .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => 
                    src.ProductType == ProductType.Fabric 
                        ? (src.Stock != null && src.Stock.Quantity > 0)
                        : (src.ProductVariants != null && src.ProductVariants.Any(v => v.StockQuantity > 0))
                ))
                .ReverseMap();

            // Varyant ve Özellikler
            CreateMap<ProductVariant, CreateProductVariantDto>().ReverseMap();
            CreateMap<ProductVariant, ResultProductVariantDto>().ReverseMap();
            CreateMap<ProductProperty, CreateProductPropertyDto>().ReverseMap();
            CreateMap<ProductProperty, ResultProductPropertyDto>().ReverseMap();
            CreateMap<Stock, ResultStockDto>().ReverseMap();

            // --- CATEGORY ---
            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<Category, UpdateCategoryDto>().ReverseMap();
            CreateMap<Category, ResultCategoryDto>().ReverseMap();

            // --- BLOG ---
            CreateMap<Blog, CreateBlogDto>().ReverseMap();
            CreateMap<Blog, UpdateBlogDto>().ReverseMap();
            CreateMap<Blog, ResultBlogDto>().ReverseMap();
            CreateMap<Blog, ResultBlogListDto>().ReverseMap();
            CreateMap<BlogCategory, ResultBlogCategoryDto>().ReverseMap();

            // --- ORDER & CART ---
            CreateMap<Order, CreateOrderDto>().ReverseMap();
            CreateMap<Order, ResultOrderDto>().ReverseMap();
            CreateMap<Order, ResultOrderListDto>().ReverseMap();

            CreateMap<OrderItem, ResultOrderItemDto>().ReverseMap();

            CreateMap<ShoppingCartItem, AddCartItemDto>().ReverseMap();
            CreateMap<ShoppingCartItem, ResultCartItemDto>().ReverseMap();

            // --- SUPPORT & CONTACT ---
            CreateMap<ContactMessage, CreateContactMessageDto>().ReverseMap();
            CreateMap<ContactMessage, ResultContactMessageDto>().ReverseMap();

            CreateMap<SupportTicket, CreateSupportTicketDto>().ReverseMap();
            CreateMap<SupportTicket, ResultSupportTicketDto>().ReverseMap();

            // --- USER & ADDRESS ---
            // 1. DTO'dan Entity'e (Create/Update)
            // 2. Entity'den DTO'ya (Result)
           
                    // --- USER & ADDRESS ---
            // 1. CreateAddressDto'yu Address entity'sine ve tersine eşler (Yeni Kayıt için)
            CreateMap<Address, CreateAddressDto>().ReverseMap();

            // 2. UpdateAddressDto'yu Address entity'sine ve tersine eşler (Güncelleme için)
            CreateMap<Address, UpdateAddressDto>().ReverseMap();

            // 3. Address entity'sini ResultAddressDto'ya ve tersine eşler (Listeleme için)
            CreateMap<Address, ResultAddressDto>().ReverseMap();

            CreateMap<CustomerSocial, ResultCustomerSocialDto>().ReverseMap();
            CreateMap<CustomerSocial, CreateCustomerSocialDto>().ReverseMap();
            CreateMap<CustomerSocial, UpdateCustomerSocialDto>().ReverseMap();
          
            CreateMap<ProductDisplay, CreateProductDisplayDto>().ReverseMap();

            CreateMap<StockLog, ResultStockLogDto>();

        }
    }
}