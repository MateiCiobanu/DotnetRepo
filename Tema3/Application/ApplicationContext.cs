using Tema3.Domain.Entities;
using Tema3.Repositories;

namespace Tema3.Application;

public class ApplicationContext
{
    private readonly IProductRepository _repo;

    public ApplicationContext(IProductRepository repo)
    {
        _repo = repo;
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct)
        => _repo.GetAllAsync(ct);

    public Task<bool> SkuExistsAsync(string sku, CancellationToken ct)
        => _repo.SkuExistsAsync(sku, ct);

    public async Task<bool> NameExistsForBrandAsync(string name, string brand, CancellationToken ct)
    {
        var all = await _repo.GetAllAsync(ct);
        return all.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(p.Brand, brand, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<int> CountCreatedTodayAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var all = await _repo.GetAllAsync(ct);
        return all.Count(p => p.CreatedAt.Date == today);
    }
}