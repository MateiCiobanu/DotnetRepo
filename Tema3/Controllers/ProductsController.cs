using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Tema3.Application.Caching;
using Tema3.Application.Dtos;
using Tema3.Application.Handlers;
using Tema3.Application.Requests;
using Tema3.Application.Services;
using Tema3.Domain.Entities;
using Tema3.Domain.Enums;

namespace Tema3.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
[Tags("Products")]
public class ProductsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ICreateProductHandler _create;
    private readonly IBatchCreateProductHandler _batchCreate;
    private readonly IProductCacheService _cacheService;
    private readonly IProductMetricsService _metricsService;

    public ProductsController(
        IMapper mapper,
        ICreateProductHandler create,
        IBatchCreateProductHandler batchCreate,
        IProductCacheService cacheService,
        IProductMetricsService metricsService)
    {
        _mapper = mapper;
        _create = create;
        _batchCreate = batchCreate;
        _cacheService = cacheService;
        _metricsService = metricsService;
    }

    [HttpPost("profile-legacy")]
    public ActionResult<ProductProfileDto> CreateProfileLegacy([FromBody] CreateProductProfileRequest request)
    {
        var entity = _mapper.Map<Product>(request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;

        var dto = _mapper.Map<ProductProfileDto>(entity);
        return Ok(dto);
    }

    [HttpPost("profile")]
    [ProducesResponseType(typeof(ProductProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductProfileDto>> CreateProfile([FromBody] CreateProductProfileRequest request, CancellationToken ct)
    {
        try
        {
            var dto = await _create.Handle(request, ct);
            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    // BONUS 1
    [HttpGet("by-category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<ProductProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductProfileDto>>> GetProductsByCategory(
        [FromRoute] ProductCategory category,
        CancellationToken ct)
    {
        try
        {
            var products = await _cacheService.GetProductsByCategoryAsync(category, ct);
            var dtos = _mapper.Map<IEnumerable<ProductProfileDto>>(products);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    // BONUS 2
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ProductMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductMetricsDto>> GetProductMetrics(CancellationToken ct)
    {
        try
        {
            var metrics = await _metricsService.GetProductMetricsAsync(ct);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    // BONUS 4
    [HttpPost("batch")]
    [ProducesResponseType(typeof(BatchProductResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchProductResultDto>> CreateProductBatch(
        [FromBody] BatchCreateProductRequest request,
        CancellationToken ct)
    {
        try
        {
            if (request.Products == null || !request.Products.Any())
            {
                return BadRequest(new { error = "Products list cannot be empty." });
            }

            var result = await _batchCreate.HandleBatchAsync(request, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }
}