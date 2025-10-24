using Tema3.Domain.Entities;

namespace Tema3.Repositories;

public interface IProductRepository
{
    Task<bool> SkuExistsAsync(string sku, CancellationToken ct);
    Task AddAsync(Product product, CancellationToken ct);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct);
}