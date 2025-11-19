using Tema3.Domain.Enums;

namespace Tema3.Application.Logging;

public record ProductCreationMetrics
{
    public required string OperationId { get; init; }
    public required string ProductName { get; init; }
    public required string SKU { get; init; }
    public required ProductCategory Category { get; init; }
    public required TimeSpan ValidationDuration { get; init; }
    public required TimeSpan DatabaseSaveDuration { get; init; }
    public required TimeSpan TotalDuration { get; init; }
    public required bool Success { get; init; }
    public string? ErrorReason { get; init; }
}

