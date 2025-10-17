using FluentValidation;
using Tema2.Persistence;
using Tema2.Requests;
namespace Tema2.Handlers;

public class CreateBookHandler
{
    private readonly BookManagementContext _context;
    private readonly IValidator<CreateBookRequest> _validator;
    
    public CreateBookHandler(BookManagementContext context, IValidator<CreateBookRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> HandleAsync(CreateBookRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new
            {
                field = e.PropertyName,
                message = e.ErrorMessage
            });

            return Results.BadRequest(errors);
        }
        
        var book = new Book();
        book.Title = request.Title;
        book.Author = request.Author;
        book.Year = request.Year;
        
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return Results.Created($"/books/{book.Id}", book);
    }
}