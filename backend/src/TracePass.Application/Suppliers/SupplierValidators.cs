using FluentValidation;

namespace TracePass.Application.Suppliers;

public class CreateSupplierValidator : AbstractValidator<CreateSupplierRequest>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(200).WithMessage("Supplier name cannot exceed 200 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail))
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0.0m, 5.0m)
            .WithMessage("Rating must be between 0.0 and 5.0.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public class UpdateSupplierProfileValidator : AbstractValidator<UpdateSupplierProfileRequest>
{
    public UpdateSupplierProfileValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(0.0m, 5.0m)
            .WithMessage("Rating must be between 0.0 and 5.0.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters.");
    }
}
