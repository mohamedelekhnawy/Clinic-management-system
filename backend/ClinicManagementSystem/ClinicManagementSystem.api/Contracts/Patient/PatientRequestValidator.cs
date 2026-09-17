using FluentValidation;
using ClinicManagementSystem.api.Contracts.Profile;

namespace ClinicManagementSystem.api.Contracts.Patient;

public class PatientRequestValidator : AbstractValidator<PatientRequest>
{
    public PatientRequestValidator()
    {
        RuleFor(x => x.Profile)
            .NotNull().WithMessage("Profile is required")
            .SetValidator(new ProfileRequestValidator());

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateOnly.FromDateTime(DateTime.Today)).WithMessage("Date of birth must be in the past");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value");

        RuleFor(x => x.Address_En)
            .MaximumLength(500).WithMessage("English address cannot exceed 500 characters");

        RuleFor(x => x.Address_Ar)
            .MaximumLength(500).WithMessage("Arabic address cannot exceed 500 characters");

        RuleFor(x => x.EmergencyContactName)
            .MaximumLength(200).WithMessage("Emergency contact name cannot exceed 200 characters");

        RuleFor(x => x.EmergencyContactPhone)
            .MaximumLength(11).WithMessage("Emergency contact phone cannot exceed 11 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }
}
