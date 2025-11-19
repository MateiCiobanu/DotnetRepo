namespace Tema3.Application.Dtos;

public class BatchProductResultDto
{
    public int TotalRequests { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public List<ProductResultItem> Results { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

public class ProductResultItem
{
    public int Index { get; set; }
    public bool Success { get; set; }
    public ProductProfileDto? Product { get; set; }
    public string? Error { get; set; }
    public TimeSpan ProcessingDuration { get; set; }
}

