using FluentValidation;
using Tema2.Requests;
using Tema2.Persistence;

namespace Tema2.Handlers;

public class DeleteBookHandler
{
    private readonly BookManagementContext _context;
    private readonly IValidator<DeleteBookRequest> _validator;
    
    public DeleteBookHandler(BookManagementContext context, IValidator<DeleteBookRequest> validator)
    {
        _context = context;
        _validator = validator;
    }
    
    public async Task<IResult> HandleAsync(DeleteBookRequest request)
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

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return Results.NoContent();
    }
}