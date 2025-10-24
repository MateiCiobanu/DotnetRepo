using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tema3.Application.Validation.Attributes;

public class ValidSKUAttribute : ValidationAttribute, IClientModelValidator
{
    private static readonly Regex Pattern = new(@"^[A-Za-z0-9\-]{5,20}$", RegexOptions.Compiled);

    public ValidSKUAttribute()
    {
        ErrorMessage = "SKU must be alphanumeric with hyphens, 5 to 20 characters.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Lăsăm [Required] sau validatorul avansat să decidă dacă e gol
        if (value is null) return ValidationResult.Success;

        var input = value.ToString()!.Replace(" ", string.Empty);
        return Pattern.IsMatch(input)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage);
    }

    // Client-side attributes pentru formulare MVC
    public void AddValidation(ClientModelValidationContext context)
    {
        Merge(context, "data-val", "true");
        Merge(context, "data-val-validsku", ErrorMessage!);
        Merge(context, "data-val-validsku-pattern", Pattern.ToString());
        Merge(context, "data-val-validsku-trim", "true"); // indicativ, dacă vrei să-l folosești în js
    }

    private static void Merge(ClientModelValidationContext ctx, string key, string value)
    {
        if (!ctx.Attributes.ContainsKey(key))
            ctx.Attributes.Add(key, value);
    }
}