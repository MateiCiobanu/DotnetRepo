using FluentValidation;
using Tema2.Persistence;
using Tema2.Requests;
namespace Tema2.Handlers;

public class UpdateBookHandler
{
    private readonly BookManagementContext _context;
    private readonly IValidator<UpdateBookRequest> _validator;
    
    public UpdateBookHandler(BookManagementContext context, IValidator<UpdateBookRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> HandleAsync(int id, UpdateBookRequest request)
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
        
        var book = await _context.Books.FindAsync(id);
        if (book == null) {
            return Results.NotFound();
        }
        book.Title = request.Title;
        book.Author = request.Author;
        book.Year = request.Year;

        await _context.SaveChangesAsync();
        return Results.Ok(new
        {
            message = "Book was updated successfully",
            book = book
        });
    }
}