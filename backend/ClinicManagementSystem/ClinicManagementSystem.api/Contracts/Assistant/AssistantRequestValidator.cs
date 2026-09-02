namespace ClinicManagementSystem.api.Contracts.Assistant;

public class AssistantRequestValidator : AbstractValidator<AssistantRequest>
{
    public AssistantRequestValidator()
    {
        // Clinic ID
        RuleFor(x => x.ClinicId)
            .GreaterThan(0).WithMessage("Clinic ID must be greater than 0");

        // English First Name
        RuleFor(x => x.FirstName_En)
            .NotEmpty().WithMessage("English first name is required")
            .MaximumLength(100).WithMessage("English first name must not exceed 100 characters")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("English first name must contain only English letters");

        // Arabic First Name
        RuleFor(x => x.FirstName_Ar)
            .NotEmpty().WithMessage("Arabic first name is required")
            .MaximumLength(100).WithMessage("Arabic first name must not exceed 100 characters")
            .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage("Arabic first name must contain only Arabic letters");

        // English Last Name
        RuleFor(x => x.LastName_En)
            .NotEmpty().WithMessage("English last name is required")
            .MaximumLength(100).WithMessage("English last name must not exceed 100 characters")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("English last name must contain only English letters");

        // Arabic Last Name
        RuleFor(x => x.LastName_Ar)
            .NotEmpty().WithMessage("Arabic last name is required")
            .MaximumLength(100).WithMessage("Arabic last name must not exceed 100 characters")
            .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage("Arabic last name must contain only Arabic letters");

        // Phone
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^(010|011|012|015)\d{8}$").WithMessage("Invalid Egyptian phone number format. Must start with 010, 011, 012, or 015 followed by 8 digits");

        // Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters");
    }
}
