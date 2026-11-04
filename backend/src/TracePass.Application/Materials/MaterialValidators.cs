using FluentValidation;

namespace TracePass.Application.Materials;

public class CreateMaterialValidator : AbstractValidator<CreateMaterialRequest>
{
    public CreateMaterialValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Material name is required.")
            .MaximumLength(200).WithMessage("Material name cannot exceed 200 characters.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("A valid material category must be selected.");

        RuleFor(x => x.OriginCountry)
            .NotEmpty().WithMessage("Origin country is required.")
            .MaximumLength(100).WithMessage("Origin country cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public class UpdateMaterialValidator : AbstractValidator<UpdateMaterialRequest>
{
    public UpdateMaterialValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Material name is required.")
            .MaximumLength(200).WithMessage("Material name cannot exceed 200 characters.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("A valid material category must be selected.");

        RuleFor(x => x.OriginCountry)
            .NotEmpty().WithMessage("Origin country is required.")
            .MaximumLength(100).WithMessage("Origin country cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}
