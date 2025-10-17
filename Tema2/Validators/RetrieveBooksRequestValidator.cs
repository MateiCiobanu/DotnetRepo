using FluentValidation;
using Tema2.Requests;

namespace Tema2.Validators;

public class RetrieveBooksRequestValidator : AbstractValidator<RetrieveBooksRequest>
{
    public RetrieveBooksRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
        
        RuleFor(x => x.Author)
            .MaximumLength(50).WithMessage("Author filter must not exceed 50 characters.");
        
        RuleFor(x => x.SortBy)
            .Must(s => s is null or "title" or "year").WithMessage("SortBy must be 'title' or 'year'.");

        RuleFor(x => x.SortDirection)
            .Must(s => s is null or "asc" or "desc").WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}