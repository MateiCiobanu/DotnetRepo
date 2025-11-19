using AutoMapper;
using Microsoft.Extensions.Localization;
using Tema3.Application.Dtos;
using Tema3.Domain.Entities;

namespace Tema3.Application.Mapping.Resolvers;

public class LocalizedProductAgeResolver : IValueResolver<Product, ProductProfileDto, string>
{
    private readonly IStringLocalizer _localizer;

    public LocalizedProductAgeResolver(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("ProductResources", "Tema3");
    }

    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var now = DateTime.UtcNow;
        if (source.ReleaseDate > now) return _localizer["Age_NewRelease"];

        var days = (now - source.ReleaseDate).Days;

        if (days < 30) return _localizer["Age_NewRelease"];
        if (days < 365) return string.Format(_localizer["Age_MonthsOld"], days / 30);
        if (days < 1825) return string.Format(_localizer["Age_YearsOld"], days / 365);
        if (days == 1825) return _localizer["Age_Classic"];

        return string.Format(_localizer["Age_YearsOld"], days / 365);
    }
}

