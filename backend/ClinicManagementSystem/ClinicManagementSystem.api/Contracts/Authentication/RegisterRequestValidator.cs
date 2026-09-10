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

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);
        }
    }
}
