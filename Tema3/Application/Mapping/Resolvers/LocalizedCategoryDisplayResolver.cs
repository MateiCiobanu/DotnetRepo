using AutoMapper;
using Microsoft.Extensions.Localization;
using Tema3.Application.Dtos;
using Tema3.Domain.Entities;
using Tema3.Domain.Enums;

namespace Tema3.Application.Mapping.Resolvers;

public class LocalizedCategoryDisplayResolver : IValueResolver<Product, ProductProfileDto, string>
{
    private readonly IStringLocalizer _localizer;

    public LocalizedCategoryDisplayResolver(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("ProductResources", "Tema3");
    }

    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var key = source.Category switch
        {
            ProductCategory.Electronics => "Category_Electronics",
            ProductCategory.Clothing => "Category_Clothing",
            ProductCategory.Books => "Category_Books",
            ProductCategory.Home => "Category_Home",
            _ => "Category_Uncategorized"
        };

        return _localizer[key];
    }
}

