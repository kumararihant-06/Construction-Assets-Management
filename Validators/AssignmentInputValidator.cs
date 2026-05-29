using ConstructionAssetAPI.Endpoints;
using FluentValidation;

namespace ConstructionAssetAPI.Validators;

public class AssignmentInputValidator : AbstractValidator<AssignmentInput>
{
    public AssignmentInputValidator()
    {
        RuleFor(a => a.EquipmentId).GreaterThan(0);
        RuleFor(a => a.JobSiteId).GreaterThan(0);
        RuleFor(a => a.AssignedDate).NotEmpty();
        RuleFor(a => a.Notes).MaximumLength(500);
    }
}