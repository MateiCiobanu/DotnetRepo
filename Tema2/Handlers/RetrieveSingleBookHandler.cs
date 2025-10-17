using FluentValidation;
using Tema2.Persistence;
using Tema2.Requests;

namespace Tema2.Handlers;

public class RetrieveSingleBookHandler
{
    private readonly BookManagementContext _context;
    private readonly IValidator<RetrieveSingleBookRequest> _validator;

    public RetrieveSingleBookHandler(BookManagementContext context, IValidator<RetrieveSingleBookRequest> validator)
    {
        _context = context;
        _validator = validator;
    }
    
    public async Task<IResult> HandleAsync(RetrieveSingleBookRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => new
            {
                field = e.PropertyName,
                message = e.ErrorMessage
            });
            return Results.BadRequest(errors);
        }
        
        var book = await _context.Books.FindAsync(request.Id);
        if (book == null) {
            return Results.NotFound();
        }
        return Results.Ok(book);
    }
}