using FluentValidation;
using Tema2.Requests;
namespace Tema2.Validators;

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters");
        
        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required")
            .MaximumLength(50).WithMessage("Author name must not exceed 50 characters");
        
        RuleFor(x => x.Year)
            .InclusiveBetween(0, DateTime.Now.Year).WithMessage($"Year must be between 0 and {DateTime.Now.Year}");
    }
}