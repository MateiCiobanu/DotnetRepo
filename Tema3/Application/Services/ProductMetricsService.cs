using Microsoft.Extensions.Logging;
using Tema3.Application.Dtos;
using Tema3.Domain.Enums;
using Tema3.Repositories;

namespace Tema3.Application.Services;

public class ProductMetricsService : IProductMetricsService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductMetricsService> _logger;

    public ProductMetricsService(
        IProductRepository repository,
        ILogger<ProductMetricsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProductMetricsDto> GetProductMetricsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Generating product metrics dashboard...");

        var allProducts = await _repository.GetAllAsync(ct);
        var productsList = allProducts.ToList();

        var totalProducts = productsList.Count;
        var totalAvailable = productsList.Count(p => p.IsAvailable);
        var totalOutOfStock = productsList.Count(p => !p.IsAvailable);
        var totalInventoryValue = productsList.Sum(p => p.Price * p.StockQuantity);
        var totalStock = productsList.Sum(p => p.StockQuantity);

        var categoryMetrics = new Dictionary<ProductCategory, CategoryMetrics>();
        foreach (ProductCategory category in Enum.GetValues(typeof(ProductCategory)))
        {
            var categoryProducts = productsList.Where(p => p.Category == category).ToList();

            if (categoryProducts.Any())
            {
                categoryMetrics[category] = new CategoryMetrics
                {
                    Category = category,
                    ProductCount = categoryProducts.Count,
                    AvailableCount = categoryProducts.Count(p => p.IsAvailable),
                    AveragePrice = categoryProducts.Average(p => p.Price),
                    TotalValue = categoryProducts.Sum(p => p.Price * p.StockQuantity),
                    TotalStock = categoryProducts.Sum(p => p.StockQuantity)
                };
            }
        }

        var topProducts = productsList
            .OrderByDescending(p => p.Price * p.StockQuantity)
            .Take(10)
            .Select(p => new TopProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Brand = p.Brand,
                Category = p.Category,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                TotalValue = p.Price * p.StockQuantity
            })
            .ToList();

        var metrics = new ProductMetricsDto
        {
            TotalProducts = totalProducts,
            TotalAvailableProducts = totalAvailable,
            TotalOutOfStock = totalOutOfStock,
            TotalInventoryValue = totalInventoryValue,
            TotalStockQuantity = totalStock,
            CategoryMetrics = categoryMetrics,
            TopValueProducts = topProducts,
            GeneratedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Product metrics generated: Total={TotalProducts}, Available={Available}, Value={Value:C}",
            totalProducts, totalAvailable, totalInventoryValue);

        return metrics;
    }
}

