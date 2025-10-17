using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Tema2.Persistence;
using Tema2.Requests;
using Tema2.Handlers;
using Tema2.Validators;
using Microsoft.AspNetCore.Mvc;
using Tema2.Middleware;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<BookManagementContext>(options =>
    options.UseSqlite("Data Source=books.db"));

builder.Services.AddValidatorsFromAssemblyContaining<CreateBookRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteBookRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RetrieveBooksRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RetrieveSingleBookRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateBookRequestValidator>();

builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<RetrieveSingleBookHandler>();
builder.Services.AddScoped<RetrieveBooksHandler>();
builder.Services.AddScoped<DeleteBookHandler>();
builder.Services.AddScoped<UpdateBookHandler>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/books", async (CreateBookRequest request, CreateBookHandler handler) =>
{
    return await handler.HandleAsync(request);
});
app.MapGet("/books/{id}", async (int id, RetrieveSingleBookHandler handler)=>
{
    var request = new RetrieveSingleBookRequest(id);
    return await handler.HandleAsync(request);
});
app.MapGet("/books", async ([AsParameters] RetrieveBooksRequest request, RetrieveBooksHandler handler) =>
{
    return await handler.HandleAsync(request);
});
app.MapDelete("/books/{id}", async (int id, DeleteBookHandler handler) =>
{
    var request = new DeleteBookRequest(id);
    return await handler.HandleAsync(request);
});
app.MapPut("/books/{id}", async (int id, UpdateBookRequest request, UpdateBookHandler handler) =>
{
    return await handler.HandleAsync(id, request);
});
app.Run();

