using AutoMapper;
using Tema3.Application.Dtos;
using Tema3.Domain.Entities;

namespace Tema3.Application.Mapping.Resolvers;

public class ProductAgeResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var now = DateTime.UtcNow;
        if (source.ReleaseDate > now) return "New Release";

        var days = (now - source.ReleaseDate).Days;

        if (days < 30) return "New Release";
        if (days < 365) return $"{days / 30} months old";
        if (days < 1825) return $"{days / 365} years old";
        if (days == 1825) return "Classic";

        return $"{days / 365} years old";
    }
}