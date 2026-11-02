using FluentValidation;

namespace TracePass.Application.Organizations;

public class OrganizationValidator : AbstractValidator<OrganizationRequest>
{
    public OrganizationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(100);

        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.ContactEmail)
            .MaximumLength(256);

        RuleFor(x => x.ContactPhone)
            .MaximumLength(30);

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}