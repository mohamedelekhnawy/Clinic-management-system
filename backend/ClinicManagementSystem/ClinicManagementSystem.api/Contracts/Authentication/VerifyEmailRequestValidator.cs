using FluentValidation;

namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
    {
        public VerifyEmailRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Verification code is required")
                .Length(6).WithMessage("Verification code must be 6 digits")
                .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits");
        }
    }
}
