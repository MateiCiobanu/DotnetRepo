using System.ComponentModel.DataAnnotations;
using Tema3.Application.Validation.Attributes;
using Tema3.Domain.Enums;

namespace Tema3.Application.Requests;

public class CreateProductProfileRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(1)]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "Brand is required.")]
    [MinLength(2)]
    [MaxLength(100)]
    public string Brand { get; set; } = default!;

    [Required(ErrorMessage = "SKU is required.")]
    [ValidSKU]
    public string SKU { get; set; } = default!;

    [ProductCategory(ProductCategory.Electronics, ProductCategory.Clothing, ProductCategory.Books, ProductCategory.Home)]
    public ProductCategory Category { get; set; }

    [PriceRange(0.01, 9999.99)]
    public decimal Price { get; set; }

    public DateTime ReleaseDate { get; set; }

    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; } = 1;
}