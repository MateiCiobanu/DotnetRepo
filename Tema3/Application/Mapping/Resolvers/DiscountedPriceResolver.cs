using AutoMapper;
using Tema3.Application.Dtos;
using Tema3.Domain.Entities;
using Tema3.Domain.Enums;

namespace Tema3.Application.Mapping.Resolvers;

public class DiscountedPriceResolver : IValueResolver<Product, ProductProfileDto, decimal>
{
    public decimal Resolve(Product source, ProductProfileDto destination, decimal destMember, ResolutionContext context)
    {
        return source.Category == ProductCategory.Home
            ? decimal.Round(source.Price * 0.9m, 2)
            : source.Price;
    }
}