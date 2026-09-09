using FluentValidation;

namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public class ResendVerificationCodeRequestValidator : AbstractValidator<ResendVerificationCodeRequest>
    {
        public ResendVerificationCodeRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");
        }
    }
}
