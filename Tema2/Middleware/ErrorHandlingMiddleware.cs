using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Tema2.Middleware;
public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");
            await WriteJson(ctx, StatusCodes.Status400BadRequest, new
            {
                title = "Validation failed",
                status = 400,
                traceId = ctx.TraceIdentifier,
                errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogInformation(ex, "Not found");
            await WriteJson(ctx, StatusCodes.Status404NotFound, new
            {
                title = "Not found",
                status = 404,
                detail = ex.Message,
                traceId = ctx.TraceIdentifier
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error");
            await WriteJson(ctx, StatusCodes.Status409Conflict, new
            {
                title = "Database error",
                status = 409,
                detail = "Write failed",
                traceId = ctx.TraceIdentifier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error");
            await WriteJson(ctx, StatusCodes.Status500InternalServerError, new
            {
                title = "Server error",
                status = 500,
                detail = "Unexpected error",
                traceId = ctx.TraceIdentifier
            });
        }
    }

    private static async Task WriteJson(HttpContext ctx, int statusCode, object payload)
    {
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}