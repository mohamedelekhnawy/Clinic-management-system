using FluentValidation;
using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Contracts.Doctor;

public class CreateDoctorRequestValidation : AbstractValidator<DoctorRequest>
{
    public CreateDoctorRequestValidation()
    {
        RuleFor(x => x.ClinicId)
            .GreaterThan(0).WithMessage("Clinic ID must be greater than 0");

        RuleFor(x => x.Profile)
            .NotNull().WithMessage("Profile is required")
            .SetValidator(new ProfileRequestValidator());

        RuleFor(x => x.Specialty_En)
            .NotEmpty().WithMessage("English specialty is required")
            .MaximumLength(150).WithMessage("English specialty cannot exceed 150 characters");

        RuleFor(x => x.Specialty_Ar)
            .NotEmpty().WithMessage("Arabic specialty is required")
            .MaximumLength(150).WithMessage("Arabic specialty cannot exceed 150 characters");

        RuleFor(x => x.Description_En)
            .MaximumLength(1000).WithMessage("English description cannot exceed 1000 characters");

        RuleFor(x => x.Description_Ar)
            .MaximumLength(1000).WithMessage("Arabic description cannot exceed 1000 characters");

        RuleFor(x => x.SessionPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Session price must be greater than or equal to 0");
    }
}
