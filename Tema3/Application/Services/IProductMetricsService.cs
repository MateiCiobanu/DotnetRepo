using Tema3.Application.Dtos;

namespace Tema3.Application.Services;

public interface IProductMetricsService
{
    Task<ProductMetricsDto> GetProductMetricsAsync(CancellationToken ct);
}

