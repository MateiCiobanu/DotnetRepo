using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Tema3.Application.Validation.Attributes;

public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _min;
    private readonly decimal _max;

    public PriceRangeAttribute(double min, double max)
    {
        _min = Convert.ToDecimal(min);
        _max = Convert.ToDecimal(max);
        var culture = CultureInfo.GetCultureInfo("en-US");
        ErrorMessage = $"Price must be between {_min.ToString("C2", culture)} and {_max.ToString("C2", culture)}.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return new ValidationResult(ErrorMessage);

        if (value is decimal d && d >= _min && d <= _max)
            return ValidationResult.Success;

        // suport pentru double/float venite din model binder
        if (value is double dd && (decimal)dd >= _min && (decimal)dd <= _max)
            return ValidationResult.Success;

        if (value is float ff && (decimal)ff >= _min && (decimal)ff <= _max)
            return ValidationResult.Success;

        return new ValidationResult(ErrorMessage);
    }
}