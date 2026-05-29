using ConstructionAssetAPI.Endpoints;
using FluentValidation;

namespace ConstructionAssetAPI.Validators;

public class EquipmentInputValidator : AbstractValidator<EquipmentInput>
{
    public EquipmentInputValidator()
    {
        RuleFor(e => e.Name)
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(e => e.Type)
            .NotEmpty()
            .MaximumLength(50);
        
        RuleFor(e => e.SerialNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(e => e.Status)
            .IsInEnum();
    }
}