using FluentValidation;
using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Contracts.Assistant;

public class AssistantRequestValidator : AbstractValidator<AssistantRequest>
{
    public AssistantRequestValidator()
    {
        RuleFor(x => x.ClinicId)
            .GreaterThan(0).WithMessage("Clinic ID must be greater than 0");

        RuleFor(x => x.Profile)
            .NotNull().WithMessage("Profile is required")
            .SetValidator(new ProfileRequestValidator());
    }
}
