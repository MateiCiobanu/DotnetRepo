using AutoMapper;
using Tema3.Application.Dtos;
using Tema3.Application.Mapping.Resolvers;
using Tema3.Application.Requests;
using Tema3.Domain.Entities;
using Tema3.Domain.Enums;

namespace Tema3.Application.Mapping;

// BONUS 3
public class LocalizedProductMappingProfile : Profile
{
    public LocalizedProductMappingProfile()
    {
        CreateMap<CreateProductProfileRequest, Product>()
            .ForMember(d => d.Id, o => o.MapFrom(_ => Guid.NewGuid()))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.IsAvailable, o => o.MapFrom(s => s.StockQuantity > 0))
            .ForMember(d => d.UpdatedAt, o => o.Ignore());
        
        CreateMap<Product, ProductProfileDto>()
            .ForMember(d => d.ImageUrl,
                o => o.MapFrom(s => s.Category == ProductCategory.Home ? null : s.ImageUrl))
            .ForMember(d => d.Price, o => o.MapFrom<DiscountedPriceResolver>())
            .ForMember(d => d.FormattedPrice, o => o.MapFrom<PriceFormatterResolver>())
            .ForMember(d => d.CategoryDisplayName, o => o.MapFrom<LocalizedCategoryDisplayResolver>())
            .ForMember(d => d.ProductAge, o => o.MapFrom<LocalizedProductAgeResolver>())
            .ForMember(d => d.AvailabilityStatus, o => o.MapFrom<LocalizedAvailabilityStatusResolver>())
            .ForMember(d => d.BrandInitials, o => o.MapFrom<BrandInitialsResolver>());
    }
}

