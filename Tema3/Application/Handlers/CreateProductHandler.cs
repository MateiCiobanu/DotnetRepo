using System.Diagnostics;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tema3.Application.Caching;
using Tema3.Application.Dtos;
using Tema3.Application.Logging;
using Tema3.Application.Requests;
using Tema3.Application.Validation;
using Tema3.Domain.Entities;
using Tema3.Repositories;

namespace Tema3.Application.Handlers;

public interface ICreateProductHandler
{
    Task<ProductProfileDto> Handle(CreateProductProfileRequest request, CancellationToken ct);
}

public class CreateProductHandler : ICreateProductHandler
{
    private const string CacheKey = "all_products";

    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly IProductCacheService _cacheService;
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly CreateProductProfileValidator _validator;

    public CreateProductHandler(
        IProductRepository repo,
        IMapper mapper,
        IMemoryCache cache,
        IProductCacheService cacheService,
        ILogger<CreateProductHandler> logger,
        CreateProductProfileValidator validator)
    {
        _repo = repo;
        _mapper = mapper;
        _cache = cache;
        _cacheService = cacheService;
        _logger = logger;
        _validator = validator;
    }

    public async Task<ProductProfileDto> Handle(CreateProductProfileRequest request, CancellationToken ct)
    {
        var operationStartTime = Stopwatch.GetTimestamp();

        var operationId = Guid.NewGuid().ToString("N")[..8];

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationId"] = operationId,
            ["Operation"] = "CreateProduct"
        });

        try
        {
            _logger.LogInformation(
                eventId: new EventId(LogEvents.ProductCreationStarted),
                "Product creation started. OperationId={OperationId}, Name={Name}, Brand={Brand}, SKU={SKU}, Category={Category}",
                operationId, request.Name, request.Brand, request.SKU, request.Category);

            _logger.LogInformation(
                eventId: new EventId(LogEvents.SKUValidationPerformed),
                "SKU validation performed. OperationId={OperationId}, SKU={SKU}",
                operationId, request.SKU);

            _logger.LogInformation(
                eventId: new EventId(LogEvents.StockValidationPerformed),
                "Stock validation performed. OperationId={OperationId}, StockQuantity={StockQuantity}",
                operationId, request.StockQuantity);

            var validationStartTime = Stopwatch.GetTimestamp();

            try
            {
                await _validator.ValidateAndThrowAsync(request, ct);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    eventId: new EventId(LogEvents.ProductValidationFailed),
                    "Product validation failed. OperationId={OperationId}, Name={Name}, SKU={SKU}, Category={Category}, Reason={Reason}",
                    operationId, request.Name, request.SKU, request.Category, ex.Message);
                throw;
            }

            var validationDuration = Stopwatch.GetElapsedTime(validationStartTime);

            var entity = _mapper.Map<Product>(request);

            var dbStartTime = Stopwatch.GetTimestamp();

            _logger.LogInformation(
                eventId: new EventId(LogEvents.DatabaseOperationStarted),
                "Database operation started. OperationId={OperationId}, SKU={SKU}",
                operationId, request.SKU);

            await _repo.AddAsync(entity, ct);

            _logger.LogInformation(
                eventId: new EventId(LogEvents.DatabaseOperationCompleted),
                "Database operation completed. OperationId={OperationId}, ProductId={ProductId}, SKU={SKU}",
                operationId, entity.Id, entity.SKU);

            var dbDuration = Stopwatch.GetElapsedTime(dbStartTime);

            _cacheService.InvalidateCategoryCache(request.Category);

            _logger.LogInformation(
                eventId: new EventId(LogEvents.CacheOperationPerformed),
                "Category cache invalidation performed. OperationId={OperationId}, Category={Category}",
                operationId, request.Category);

            _cache.Remove(CacheKey);

            var dto = _mapper.Map<ProductProfileDto>(entity);

            var totalDuration = Stopwatch.GetElapsedTime(operationStartTime);

            var metrics = new ProductCreationMetrics
            {
                OperationId = operationId,
                ProductName = request.Name,
                SKU = request.SKU,
                Category = request.Category,
                ValidationDuration = validationDuration,
                DatabaseSaveDuration = dbDuration,
                TotalDuration = totalDuration,
                Success = true,
                ErrorReason = null
            };

            _logger.LogProductCreationMetrics(metrics);

            return dto;
        }
        catch (Exception ex)
        {
            var totalDuration = Stopwatch.GetElapsedTime(operationStartTime);

            var errorMetrics = new ProductCreationMetrics
            {
                OperationId = operationId,
                ProductName = request.Name ?? "Unknown",
                SKU = request.SKU ?? "Unknown",
                Category = request.Category,
                ValidationDuration = TimeSpan.Zero,
                DatabaseSaveDuration = TimeSpan.Zero,
                TotalDuration = totalDuration,
                Success = false,
                ErrorReason = ex.Message
            };

            _logger.LogProductCreationMetrics(errorMetrics);

            throw;
        }
    }
}