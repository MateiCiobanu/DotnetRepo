using System.Collections.Concurrent;
using Tema3.Domain.Entities;

namespace Tema3.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private static readonly ConcurrentDictionary<string, Product> _bySku = new();

    public Task<bool> SkuExistsAsync(string sku, CancellationToken ct)
        => Task.FromResult(_bySku.ContainsKey(sku));

    public Task AddAsync(Product product, CancellationToken ct)
    {
        _bySku[product.SKU] = product;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct)
        => Task.FromResult((IReadOnlyList<Product>)_bySku.Values.ToList());
}