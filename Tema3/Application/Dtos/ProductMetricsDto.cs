using Tema3.Domain.Enums;

namespace Tema3.Application.Dtos;

public class ProductMetricsDto
{
    public int TotalProducts { get; set; }
    public int TotalAvailableProducts { get; set; }
    public int TotalOutOfStock { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int TotalStockQuantity { get; set; }
    public Dictionary<ProductCategory, CategoryMetrics> CategoryMetrics { get; set; } = new();
    public List<TopProductDto> TopValueProducts { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class CategoryMetrics
{
    public ProductCategory Category { get; set; }
    public int ProductCount { get; set; }
    public int AvailableCount { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalValue { get; set; }
    public int TotalStock { get; set; }
}

public class TopProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Brand { get; set; } = default!;
    public ProductCategory Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public decimal TotalValue { get; set; }
}

