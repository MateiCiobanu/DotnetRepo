using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tema3.Application.Logging;
using Tema3.Domain.Entities;
using Tema3.Domain.Enums;
using Tema3.Repositories;

namespace Tema3.Application.Caching;

public class ProductCacheService : IProductCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductCacheService> _logger;

    private const string AllProductsCacheKey = "all_products";
    private const string CategoryCacheKeyPrefix = "products_category_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public ProductCacheService(
        IMemoryCache cache,
        IProductRepository repository,
        ILogger<ProductCacheService> logger)
    {
        _cache = cache;
        _repository = repository;
        _logger = logger;
    }

    public string GetCacheKey(ProductCategory category)
    {
        return $"{CategoryCacheKeyPrefix}{category}";
    }

    public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(
        ProductCategory category,
        CancellationToken ct)
    {
        var cacheKey = GetCacheKey(category);

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<Product>? cachedProducts))
        {
            _logger.LogInformation(
                eventId: new EventId(LogEvents.CacheOperationPerformed),
                "Cache hit for category {Category}. CacheKey={CacheKey}",
                category, cacheKey);

            return cachedProducts!;
        }

        _logger.LogInformation(
            "Cache miss for category {Category}. Fetching from repository.",
            category);

        var allProducts = await _repository.GetAllAsync(ct);
        var categoryProducts = allProducts
            .Where(p => p.Category == category)
            .ToList()
            .AsReadOnly();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .SetPriority(CacheItemPriority.Normal);

        _cache.Set(cacheKey, categoryProducts, cacheOptions);

        _logger.LogInformation(
            eventId: new EventId(LogEvents.CacheOperationPerformed),
            "Cached {Count} products for category {Category}. CacheKey={CacheKey}",
            categoryProducts.Count, category, cacheKey);

        return categoryProducts;
    }

    public void InvalidateCategoryCache(ProductCategory category)
    {
        var cacheKey = GetCacheKey(category);
        _cache.Remove(cacheKey);

        _logger.LogInformation(
            eventId: new EventId(LogEvents.CacheOperationPerformed),
            "Category cache invalidated. Category={Category}, CacheKey={CacheKey}",
            category, cacheKey);
    }

    public void InvalidateAllCache()
    {
        foreach (ProductCategory category in Enum.GetValues(typeof(ProductCategory)))
        {
            InvalidateCategoryCache(category);
        }

        _cache.Remove(AllProductsCacheKey);

        _logger.LogInformation(
            eventId: new EventId(LogEvents.CacheOperationPerformed),
            "All product caches invalidated.");
    }
}

