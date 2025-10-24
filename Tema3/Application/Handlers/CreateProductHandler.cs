using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tema3.Application.Dtos;
using Tema3.Application.Requests;
using Tema3.Application.Validation;   // <- nou
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
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly CreateProductProfileValidator _validator;  // <- nou

    public CreateProductHandler(
        IProductRepository repo,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<CreateProductHandler> logger,
        CreateProductProfileValidator validator)   // <- nou
    {
        _repo = repo;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
        _validator = validator;                    // <- nou
    }

    public async Task<ProductProfileDto> Handle(CreateProductProfileRequest request, CancellationToken ct)
    {
        _logger.LogInformation(
            "CreateProduct started. Name={Name}, Brand={Brand}, Category={Category}, SKU={SKU}",
            request.Name, request.Brand, request.Category, request.SKU);

        // Validare avansată, async, cu toate regulile și verificările de unicitate
        await _validator.ValidateAndThrowAsync(request, ct);

        // Mapping avansat Product
        var entity = _mapper.Map<Product>(request);

        await _repo.AddAsync(entity, ct);

        // Invalidare cache
        _cache.Remove(CacheKey);

        var dto = _mapper.Map<ProductProfileDto>(entity);

        _logger.LogInformation("CreateProduct done. Id={Id}, SKU={SKU}", entity.Id, entity.SKU);
        return dto;
    }
}