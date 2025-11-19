using FluentValidation;
using Tema3.Application;
using Tema3.Application.Caching;
using Tema3.Application.Services;
using Tema3.Application.Validation;
using Tema3.Application.Mapping;
using Tema3.Application.Mapping.Resolvers;
using Tema3.Application.Handlers;
using Tema3.Repositories;
using Tema3.Middleware;

var builder = WebApplication.CreateBuilder(args);

// BONUS 3
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "ro", "fr" };
    options.SetDefaultCulture("en")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

// Register all profiles
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ProductProfile>();
    cfg.AddProfile<AdvancedProductMappingProfile>();
    cfg.AddProfile<LocalizedProductMappingProfile>();
});

// Resolvers
builder.Services.AddTransient<CategoryDisplayResolver>();
builder.Services.AddTransient<PriceFormatterResolver>();
builder.Services.AddTransient<ProductAgeResolver>();
builder.Services.AddTransient<BrandInitialsResolver>();
builder.Services.AddTransient<AvailabilityStatusResolver>();
builder.Services.AddTransient<DiscountedPriceResolver>();

// BONUS 3
builder.Services.AddTransient<LocalizedCategoryDisplayResolver>();
builder.Services.AddTransient<LocalizedAvailabilityStatusResolver>();
builder.Services.AddTransient<LocalizedProductAgeResolver>();

// Context + Validator
builder.Services.AddSingleton<ApplicationContext>();
builder.Services.AddScoped<CreateProductProfileValidator>();


builder.Services.AddValidatorsFromAssemblyContaining<CreateProductProfileValidator>();

// BONUS 1
builder.Services.AddSingleton<IProductCacheService, ProductCacheService>();

// BONUS 2
builder.Services.AddScoped<IProductMetricsService, ProductMetricsService>();

// Repo + Handler
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<ICreateProductHandler, CreateProductHandler>();

// BONUS 4
builder.Services.AddScoped<IBatchCreateProductHandler, BatchCreateProductHandler>();

var app = builder.Build();

app.UseMiddleware<CorrelationMiddleware>();

// BONUS 3
app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();