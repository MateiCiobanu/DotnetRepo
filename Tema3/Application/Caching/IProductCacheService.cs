using Tema3.Domain.Entities;
using Tema3.Domain.Enums;

namespace Tema3.Application.Caching;

public interface IProductCacheService
{
    Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(ProductCategory category, CancellationToken ct);
    void InvalidateCategoryCache(ProductCategory category);
    void InvalidateAllCache();
    string GetCacheKey(ProductCategory category);
}

