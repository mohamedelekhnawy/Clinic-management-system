namespace ClinicManagementSystem.api.Contracts.Authentication
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FirstName_EN)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);

            RuleFor(x => x.FirstName_AR)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.FirstName_AR));

            RuleFor(x => x.LastName_EN)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);

            RuleFor(x => x.LastName_AR)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.LastName_AR));

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required")
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
                .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Phone number format is invalid");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);
        }
    }
}
