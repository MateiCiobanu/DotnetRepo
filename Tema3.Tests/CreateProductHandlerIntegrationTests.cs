using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Tema3.Application;
using Tema3.Application.Dtos;
using Tema3.Application.Handlers;
using Tema3.Application.Logging;
using Tema3.Application.Mapping;
using Tema3.Application.Mapping.Resolvers;
using Tema3.Application.Requests;
using Tema3.Application.Validation;
using Tema3.Domain.Entities;
using Tema3.Domain.Enums;
using Tema3.Repositories;
using Xunit;

namespace Tema3.Tests;

public class CreateProductHandlerIntegrationTests : IDisposable
{
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<CreateProductHandler>> _loggerMock;
    private readonly Mock<ILogger<CreateProductProfileValidator>> _validatorLoggerMock;
    private readonly IProductRepository _repository;
    private readonly ApplicationContext _context;
    private readonly CreateProductProfileValidator _validator;
    private readonly CreateProductHandler _handler;
    private readonly IServiceProvider _serviceProvider;

    public CreateProductHandlerIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddTransient<CategoryDisplayResolver>();
        services.AddTransient<PriceFormatterResolver>();
        services.AddTransient<ProductAgeResolver>();
        services.AddTransient<BrandInitialsResolver>();
        services.AddTransient<AvailabilityStatusResolver>();
        services.AddTransient<DiscountedPriceResolver>();
        _serviceProvider = services.BuildServiceProvider();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductProfile>();
            cfg.AddProfile<AdvancedProductMappingProfile>();

            cfg.ConstructServicesUsing(_serviceProvider.GetService);
        });
        _mapper = mapperConfig.CreateMapper();

        _cache = new MemoryCache(new MemoryCacheOptions());

        _loggerMock = new Mock<ILogger<CreateProductHandler>>();
        _validatorLoggerMock = new Mock<ILogger<CreateProductProfileValidator>>();

        _repository = new InMemoryProductRepository();

        _context = new ApplicationContext(_repository);

        _validator = new CreateProductProfileValidator(_context, _validatorLoggerMock.Object);

        var cacheServiceLogger = new Mock<ILogger<Tema3.Application.Caching.ProductCacheService>>();
        var cacheService = new Tema3.Application.Caching.ProductCacheService(
            _cache,
            _repository,
            cacheServiceLogger.Object);

        _handler = new CreateProductHandler(
            _repository,
            _mapper,
            _cache,
            cacheService,
            _loggerMock.Object,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidElectronicsProductRequest_CreatesProductWithCorrectMappings()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Gaming Laptop with RTX GPU",
            Brand = "Tech Master",
            SKU = "TECH-LAPTOP-001",
            Category = ProductCategory.Electronics,
            Price = 1299.99m,
            ReleaseDate = DateTime.UtcNow.AddDays(-15),
            ImageUrl = "https://example.com/images/laptop.jpg",
            StockQuantity = 8
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.IsType<ProductProfileDto>(result);

        Assert.Equal("Electronics & Technology", result.CategoryDisplayName);

        Assert.Equal("TM", result.BrandInitials);

        Assert.Equal("New Release", result.ProductAge);

        Assert.StartsWith("$", result.FormattedPrice);
        Assert.Contains("1,299.99", result.FormattedPrice);

        Assert.Equal("In Stock", result.AvailabilityStatus);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == LogEvents.ProductCreationStarted),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ThrowsValidationExceptionWithLogging()
    {
        var existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Existing Product",
            Brand = "Test Brand",
            SKU = "DUPLICATE-SKU-001",
            Category = ProductCategory.Electronics,
            Price = 99.99m,
            ReleaseDate = DateTime.UtcNow.AddYears(-1),
            StockQuantity = 10,
            IsAvailable = true
        };
        await _repository.AddAsync(existingProduct, CancellationToken.None);

        var request = new CreateProductProfileRequest
        {
            Name = "New Product",
            Brand = "Another Brand",
            SKU = "DUPLICATE-SKU-001",
            Category = ProductCategory.Clothing,
            Price = 49.99m,
            ReleaseDate = DateTime.UtcNow,
            StockQuantity = 5
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _handler.Handle(request, CancellationToken.None));

        Assert.Contains("unique", exception.Message, StringComparison.OrdinalIgnoreCase);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.Is<EventId>(e => e.Id == LogEvents.ProductValidationFailed),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_HomeProductRequest_AppliesDiscountAndConditionalMapping()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Modern Sofa Set",
            Brand = "Home Comfort",
            SKU = "HOME-SOFA-001",
            Category = ProductCategory.Home,
            Price = 199.99m,
            ReleaseDate = DateTime.UtcNow.AddYears(-1),
            ImageUrl = "https://example.com/images/sofa.jpg",
            StockQuantity = 3
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal("Home & Garden", result.CategoryDisplayName);

        var expectedDiscountedPrice = Math.Round(199.99m * 0.9m, 2);
        Assert.Equal(expectedDiscountedPrice, result.Price);

        Assert.Null(result.ImageUrl);

        Assert.Contains("179.99", result.FormattedPrice);
    }

    public void Dispose()
    {
        _cache?.Dispose();

        (_serviceProvider as IDisposable)?.Dispose();

        GC.SuppressFinalize(this);
    }
}

