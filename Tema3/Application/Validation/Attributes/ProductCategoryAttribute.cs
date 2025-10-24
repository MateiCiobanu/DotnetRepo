using System.ComponentModel.DataAnnotations;
using Tema3.Domain.Enums;

namespace Tema3.Application.Validation.Attributes;

public class ProductCategoryAttribute : ValidationAttribute
{
    private readonly HashSet<ProductCategory> _allowed;

    public ProductCategoryAttribute(params ProductCategory[] allowed)
    {
        _allowed = allowed is { Length: > 0 } 
            ? new HashSet<ProductCategory>(allowed) 
            : new HashSet<ProductCategory>((ProductCategory[])Enum.GetValues(typeof(ProductCategory)));

        ErrorMessage = $"Category must be one of: {string.Join(", ", _allowed)}.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return new ValidationResult(ErrorMessage);
        if (value is ProductCategory cat && _allowed.Contains(cat)) return ValidationResult.Success;

        // suport și pentru valori trimise ca int
        if (value is int i && _allowed.Contains((ProductCategory)i)) return ValidationResult.Success;

        return new ValidationResult(ErrorMessage);
    }
}