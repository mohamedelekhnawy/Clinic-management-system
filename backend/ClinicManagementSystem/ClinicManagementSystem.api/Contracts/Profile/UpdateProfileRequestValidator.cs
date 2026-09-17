using FluentValidation;

namespace ClinicManagementSystem.api.Contracts.Profile;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName_En)
            .NotEmpty().WithMessage("English first name is required")
            .MaximumLength(100).WithMessage("English first name cannot exceed 100 characters");

        RuleFor(x => x.FirstName_Ar)
            .NotEmpty().WithMessage("Arabic first name is required")
            .MaximumLength(100).WithMessage("Arabic first name cannot exceed 100 characters");

        RuleFor(x => x.LastName_En)
            .NotEmpty().WithMessage("English last name is required")
            .MaximumLength(100).WithMessage("English last name cannot exceed 100 characters");

        RuleFor(x => x.LastName_Ar)
            .NotEmpty().WithMessage("Arabic last name is required")
            .MaximumLength(100).WithMessage("Arabic last name cannot exceed 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Phone number format is invalid");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");
    }
}
