using AutoMapper;
using Tema3.Application.Dtos;
using Tema3.Application.Mapping.Resolvers;
using Tema3.Application.Requests;
using Tema3.Domain.Entities;
using Tema3.Domain.Enums;

namespace Tema3.Application.Mapping;

public class AdvancedProductMappingProfile : Profile
{
    public AdvancedProductMappingProfile()
    {
        // Request -> Entity
        CreateMap<CreateProductProfileRequest, Product>()
            .ForMember(d => d.Id, o => o.MapFrom(_ => Guid.NewGuid()))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.IsAvailable, o => o.MapFrom(s => s.StockQuantity > 0))
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        // Entity -> DTO
        CreateMap<Product, ProductProfileDto>()
            // ImageUrl condițional
            .ForMember(d => d.ImageUrl,
                o => o.MapFrom(s => s.Category == ProductCategory.Home ? null : s.ImageUrl))
            // Preț cu reducere în DTO
            .ForMember(d => d.Price, o => o.MapFrom<DiscountedPriceResolver>())
            // Formatare preț după aceeași logică
            .ForMember(d => d.FormattedPrice, o => o.MapFrom<PriceFormatterResolver>())
            // Restul resolverelor din pasul anterior
            .ForMember(d => d.CategoryDisplayName, o => o.MapFrom<CategoryDisplayResolver>())
            .ForMember(d => d.ProductAge, o => o.MapFrom<ProductAgeResolver>())
            .ForMember(d => d.BrandInitials, o => o.MapFrom<BrandInitialsResolver>())
            .ForMember(d => d.AvailabilityStatus, o => o.MapFrom<AvailabilityStatusResolver>());
    }
}