using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Tema2.Persistence;
using Tema2.Requests;

namespace Tema2.Handlers;

public class RetrieveBooksHandler
{
    private readonly BookManagementContext _context;
    private readonly IValidator<RetrieveBooksRequest> _validator;
    
    public RetrieveBooksHandler(BookManagementContext context, IValidator<RetrieveBooksRequest> validator)
    {
        _context = context;
        _validator = validator;
    }
    
    public async Task<IResult> HandleAsync(RetrieveBooksRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage });
            return Results.BadRequest(errors);
        }
        
        var query = _context.Books.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            query = query.Where(b => b.Author.ToLower().Contains(request.Author.ToLower()));
        }
        
        var sortBy = (request.SortBy ?? "title").ToLowerInvariant();
        var sortDir = (request.SortDirection ?? "asc").ToLowerInvariant();

        query = (sortBy, sortDir) switch
        {
            ("title", "asc")  => query.OrderBy(b => b.Title).ThenBy(b => b.Id),
            ("title", "desc") => query.OrderByDescending(b => b.Title).ThenBy(b => b.Id),
            ("year",  "asc")  => query.OrderBy(b => b.Year).ThenBy(b => b.Id),
            ("year",  "desc") => query.OrderByDescending(b => b.Year).ThenBy(b => b.Id),
            _                 => query.OrderBy(b => b.Title).ThenBy(b => b.Id)
        };

        var books = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
        
        return Results.Ok(books);
    }
}