using System.Globalization;
using AutoMapper;
using Tema3.Application.Dtos;
using Tema3.Domain.Entities;
using Tema3.Domain.Enums;

namespace Tema3.Application.Mapping.Resolvers;

public class PriceFormatterResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var effective = source.Category == ProductCategory.Home
            ? decimal.Round(source.Price * 0.9m, 2)
            : source.Price;

        return effective.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
    }
}