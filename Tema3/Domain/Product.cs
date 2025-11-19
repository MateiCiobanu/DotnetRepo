using Tema3.Domain.Enums;

namespace Tema3.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string Name { get; set; } = default!;
    public string Brand { get; set; } = default!;
    public string SKU { get; set; } = default!;
    public ProductCategory Category { get; set; }
    public decimal Price { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; } = 0;
}