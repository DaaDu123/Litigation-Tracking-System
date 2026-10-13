using FluentValidation;

namespace LTSBackend.Features.Cases.Queries.GetAllCases;

public class GetAllCasesValidator : AbstractValidator<GetAllCasesQuery>
{


    public GetAllCasesValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page Number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page Size must be between 1 and 100");

        RuleFor(x => x.Priority)
            .Must(x => x == null || x == "High" || x == "Medium" || x == "Low")
            .WithMessage("Priority can only be High, Medium, or Low");
    }
}