using ConstructionAssetAPI.Endpoints;
using FluentValidation;

namespace ConstructionAssetAPI.Validators;

public class EquipmentInputValidator : AbstractValidator<EquipmentInput>
{
    private static readonly string[] AllowedStatuses = {"Available", "InUse","Maintenance"};
    public EquipmentInputValidator()
    {
        RuleFor(e => e.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);
        
        RuleFor(e => e.Type)
            .NotEmpty().WithMessage("Type is required.")
            .MaximumLength(50);
        
        RuleFor(e => e.SerialNumber)
            .NotEmpty().WithMessage("Serial Number is required.")
            .MaximumLength(50);

        RuleFor(e => e.Status)
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
    }
}