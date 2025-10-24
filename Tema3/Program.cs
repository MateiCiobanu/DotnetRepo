using Tema3.Application;                     // ApplicationContext
using Tema3.Application.Validation;          // CreateProductProfileValidator
using Tema3.Application.Mapping;
using Tema3.Application.Mapping.Resolvers;
using Tema3.Application.Handlers;
using Tema3.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AdvancedProductMappingProfile>();
});

// Resolvers
builder.Services.AddTransient<CategoryDisplayResolver>();
builder.Services.AddTransient<PriceFormatterResolver>();
builder.Services.AddTransient<ProductAgeResolver>();
builder.Services.AddTransient<BrandInitialsResolver>();
builder.Services.AddTransient<AvailabilityStatusResolver>();
builder.Services.AddTransient<DiscountedPriceResolver>();

// Context + Validator
builder.Services.AddSingleton<ApplicationContext>();
builder.Services.AddTransient<CreateProductProfileValidator>();

// Repo + Handler
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<ICreateProductHandler, CreateProductHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();