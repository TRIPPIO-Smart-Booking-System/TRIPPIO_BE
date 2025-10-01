using AutoMapper;
using Trippio.Core.Domain.Entities;
using Trippio.Core.Domain.Identity;
using Trippio.Core.Models.Auth;
using Trippio.Core.Models.Basket;
using Trippio.Core.Models.Booking;
using Trippio.Core.Models.Content;
using Trippio.Core.Models.Order;
using Trippio.Core.Models.Payment;
using Trippio.Core.Models.Product;
using Trippio.Core.Models.System;

namespace Trippio.Core.Mappings
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            // Auth mappings
            CreateMap<AppUser, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src => src.LastLoginDate))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => src.Dob))
                .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => src.IsEmailVerified))
                .ForMember(dest => dest.IsFirstLogin, opt => opt.MapFrom(src => src.IsFirstLogin))
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles sẽ được set riêng

            CreateMap<RegisterRequest, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Username.ToUpper()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => false)) // ✅ Inactive until email verified
                .ForMember(dest => dest.IsFirstLogin, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.LoyaltyAmountPerPost, opt => opt.MapFrom(src => 1000));

            CreateMap<CreateUserRequest, AppUser>();
            CreateMap<UpdateUserRequest, AppUser>();

            // Product mappings
            CreateMap<Product, Models.Content.ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.InventoryQuantity, opt => opt.MapFrom(src => src.ProductInventory != null ? src.ProductInventory.Stock : 0));

            CreateMap<Models.Content.CreateProductDto, Product>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Models.Content.UpdateProductDto, Product>()
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Category mappings  
            CreateMap<Category, CategoryDto>();

            // Order mappings
            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

            // Booking mappings
            CreateMap<Booking, BookingDto>();

            // Payment mappings
            CreateMap<Payment, PaymentDto>();

            // Basket mappings
            CreateMap<Basket, BasketDto>();
        }
    }
}
