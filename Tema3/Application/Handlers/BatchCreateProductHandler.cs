using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tema3.Application.Dtos;
using Tema3.Application.Logging;
using Tema3.Application.Requests;

namespace Tema3.Application.Handlers;

public class BatchCreateProductHandler : IBatchCreateProductHandler
{
    private readonly ICreateProductHandler _singleHandler;
    private readonly ILogger<BatchCreateProductHandler> _logger;

    public BatchCreateProductHandler(
        ICreateProductHandler singleHandler,
        ILogger<BatchCreateProductHandler> logger)
    {
        _singleHandler = singleHandler;
        _logger = logger;
    }

    public async Task<BatchProductResultDto> HandleBatchAsync(
        BatchCreateProductRequest request,
        CancellationToken ct)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        var startTime = Stopwatch.GetTimestamp();

        _logger.LogInformation(
            eventId: new EventId(LogEvents.ProductCreationStarted),
            "Batch product creation started. OperationId={OperationId}, Count={Count}, Parallel={Parallel}",
            operationId, request.Products.Count, request.EnableParallelProcessing);

        var results = new ConcurrentBag<ProductResultItem>();
        var successCount = 0;
        var failureCount = 0;

        try
        {
            if (request.EnableParallelProcessing && request.Products.Count > 1)
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Min(
                        request.MaxDegreeOfParallelism,
                        Environment.ProcessorCount),
                    CancellationToken = ct
                };

                await Parallel.ForEachAsync(
                    request.Products.Select((p, i) => (Product: p, Index: i)),
                    parallelOptions,
                    async (item, ct) =>
                    {
                        var result = await ProcessSingleProductAsync(
                            item.Product,
                            item.Index,
                            ct);
                        results.Add(result);

                        if (result.Success)
                            Interlocked.Increment(ref successCount);
                        else
                            Interlocked.Increment(ref failureCount);
                    });
            }
            else
            {
                for (int i = 0; i < request.Products.Count; i++)
                {
                    var result = await ProcessSingleProductAsync(
                        request.Products[i],
                        i,
                        ct);
                    results.Add(result);

                    if (result.Success)
                        successCount++;
                    else
                        failureCount++;
                }
            }

            var totalDuration = Stopwatch.GetElapsedTime(startTime);

            var batchResult = new BatchProductResultDto
            {
                TotalRequests = request.Products.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                TotalDuration = totalDuration,
                Results = results.OrderBy(r => r.Index).ToList(),
                ProcessedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                eventId: new EventId(LogEvents.ProductCreationCompleted),
                "Batch product creation completed. OperationId={OperationId}, Success={Success}, Failure={Failure}, Duration={Duration}ms",
                operationId, successCount, failureCount, totalDuration.TotalMilliseconds);

            return batchResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                eventId: new EventId(LogEvents.ProductValidationFailed),
                ex,
                "Batch product creation failed. OperationId={OperationId}",
                operationId);
            throw;
        }
    }

    private async Task<ProductResultItem> ProcessSingleProductAsync(
        CreateProductProfileRequest request,
        int index,
        CancellationToken ct)
    {
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            var product = await _singleHandler.Handle(request, ct);
            var duration = Stopwatch.GetElapsedTime(startTime);

            return new ProductResultItem
            {
                Index = index,
                Success = true,
                Product = product,
                Error = null,
                ProcessingDuration = duration
            };
        }
        catch (Exception ex)
        {
            var duration = Stopwatch.GetElapsedTime(startTime);

            _logger.LogWarning(
                "Batch item {Index} failed: {Error}",
                index, ex.Message);

            return new ProductResultItem
            {
                Index = index,
                Success = false,
                Product = null,
                Error = ex.Message,
                ProcessingDuration = duration
            };
        }
    }
}

