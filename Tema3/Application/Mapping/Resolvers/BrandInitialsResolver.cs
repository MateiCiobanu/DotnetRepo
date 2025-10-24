using AutoMapper;
using Tema3.Application.Dtos;
using Tema3.Domain.Entities;

namespace Tema3.Application.Mapping.Resolvers;

public class BrandInitialsResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var brand = source.Brand?.Trim();
        if (string.IsNullOrWhiteSpace(brand)) return "?";

        var parts = brand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();

        var first = parts[0][0];
        var last = parts[^1][0];
        return $"{char.ToUpperInvariant(first)}{char.ToUpperInvariant(last)}";
    }
}