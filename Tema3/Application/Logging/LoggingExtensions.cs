using Microsoft.Extensions.Logging;

namespace Tema3.Application.Logging;

public static class LoggingExtensions
{
    public static void LogProductCreationMetrics(
        this ILogger logger,
        ProductCreationMetrics metrics)
    {
        logger.LogInformation(
            eventId: new EventId(LogEvents.ProductCreationCompleted),
            "Product creation metrics: " +
            "OperationId={OperationId}, " +
            "Name={ProductName}, " +
            "SKU={SKU}, " +
            "Category={Category}, " +
            "ValidationDuration={ValidationMs}ms, " +
            "DatabaseSaveDuration={DatabaseMs}ms, " +
            "TotalDuration={TotalMs}ms, " +
            "Success={Success}, " +
            "ErrorReason={ErrorReason}",
            metrics.OperationId,
            metrics.ProductName,
            metrics.SKU,
            metrics.Category,
            metrics.ValidationDuration.TotalMilliseconds,
            metrics.DatabaseSaveDuration.TotalMilliseconds,
            metrics.TotalDuration.TotalMilliseconds,
            metrics.Success,
            metrics.ErrorReason ?? "None");
    }
}

