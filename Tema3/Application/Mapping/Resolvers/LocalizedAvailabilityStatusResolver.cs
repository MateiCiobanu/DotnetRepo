using AutoMapper;
using Microsoft.Extensions.Localization;
using Tema3.Application.Dtos;
using Tema3.Domain.Entities;

namespace Tema3.Application.Mapping.Resolvers;

public class LocalizedAvailabilityStatusResolver : IValueResolver<Product, ProductProfileDto, string>
{
    private readonly IStringLocalizer _localizer;

    public LocalizedAvailabilityStatusResolver(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("ProductResources", "Tema3");
    }

    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var key = (!source.IsAvailable) ? "Status_OutOfStock" :
                  (source.StockQuantity <= 0) ? "Status_Unavailable" :
                  (source.StockQuantity == 1) ? "Status_LastItem" :
                  (source.StockQuantity <= 5) ? "Status_LimitedStock" :
                  "Status_InStock";

        return _localizer[key];
    }
}

