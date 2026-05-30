using ConstructionAssetAPI.Endpoints;
using FluentValidation;

namespace ConstructionAssetAPI.Validators;

public class JobSiteInputValidator : AbstractValidator<JobSiteInput>
{
    public JobSiteInputValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(s => s.Location)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(s => s.StartDate)
            .NotEmpty();

        RuleFor(s => s.EndDate)
            .GreaterThan(s => s.StartDate)
            .When(s => s.EndDate.HasValue)
            .WithMessage("EndDate must be after StartDate.");
    }
}