using FluentValidation;
using Tema2.Requests;

namespace Tema2.Validators;

public class RetrieveSingleBookRequestValidator : AbstractValidator<RetrieveSingleBookRequest>
{
    public RetrieveSingleBookRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0");
    }
}