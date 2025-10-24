using System.Globalization;
using System.Text.RegularExpressions;
using AutoMapper;
using Tema3.Application.Dtos;
using Tema3.Application.Requests;
using Tema3.Domain.Entities;

namespace Tema3.Application.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Entity -> DTO
        CreateMap<Product, ProductProfileDto>()
            .ForMember(d => d.CategoryDisplayName, o => o.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.FormattedPrice, o => o.MapFrom(s => FormatPrice(s.Price)))
            .ForMember(d => d.ProductAge, o => o.MapFrom(s => ComputeAge(s.ReleaseDate)))
            .ForMember(d => d.BrandInitials, o => o.MapFrom(s => MakeInitials(s.Brand)))
            .ForMember(d => d.AvailabilityStatus, o => o.MapFrom(s => MakeAvailability(s.IsAvailable, s.StockQuantity)));

        // Request -> Entity
        CreateMap<CreateProductProfileRequest, Product>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.IsAvailable, o => o.MapFrom(s => s.StockQuantity > 0));
    }

    private static string FormatPrice(decimal price)
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        return price.ToString("C", culture);
    }

    private static string ComputeAge(DateTime release)
    {
        var now = DateTime.UtcNow;
        if (release > now) return "0 days";
        var span = now - release;

        var years = (int)(span.Days / 365.25);
        var months = (int)((span.Days % 365.25) / 30.44);
        var days = span.Days - years * 365 - months * 30;

        if (years > 0 && months > 0) return $"{years}y {months}m";
        if (years > 0) return $"{years}y";
        if (months > 0) return $"{months}m {days}d";
        return $"{days}d";
    }

    private static string MakeInitials(string brand)
    {
        if (string.IsNullOrWhiteSpace(brand)) return string.Empty;
        var parts = Regex.Split(brand.Trim(), "\\s+")
            .Where(p => p.Length > 0)
            .Take(3)
            .Select(p => char.ToUpperInvariant(p[0]));
        return new string(parts.ToArray());
    }

    private static string MakeAvailability(bool isAvailable, int stock)
    {
        if (!isAvailable || stock <= 0) return "Out of stock";
        if (stock < 5) return "Low stock";
        return "In stock";
    }
}