namespace ClinicManagementSystem.api.Contracts.Patient;

public class PatientRequestValidator : AbstractValidator<PatientRequest>
{
    public PatientRequestValidator()
    {
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

        // Date of Birth
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateOnly.FromDateTime(DateTime.Today)).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateOnly.FromDateTime(DateTime.Today.AddYears(-150))).WithMessage("Invalid date of birth");

        // Gender
        RuleFor(x => x.Gender)
            .Must(g => g == 1 || g == 2).WithMessage("Invalid gender value. Must be 1 (Male) or 2 (Female)");

        // Phone
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^(010|011|012|015)\d{8}$").WithMessage("Invalid Egyptian phone number format. Must start with 010, 011, 012, or 015 followed by 8 digits");

        // Email (Optional)
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        // Address English (Optional)
        RuleFor(x => x.Address_En)
            .MaximumLength(500).WithMessage("English address must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Address_En));

        // Address Arabic (Optional)
        RuleFor(x => x.Address_Ar)
            .MaximumLength(500).WithMessage("Arabic address must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Address_Ar));

        // Emergency Contact Name (Optional)
        RuleFor(x => x.EmergencyContactName)
            .MaximumLength(200).WithMessage("Emergency contact name must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.EmergencyContactName));

        // Emergency Contact Phone (Optional)
        RuleFor(x => x.EmergencyContactPhone)
            .Matches(@"^(010|011|012|015)\d{8}$").WithMessage("Invalid Egyptian phone number format")
            .When(x => !string.IsNullOrWhiteSpace(x.EmergencyContactPhone));

        // Notes (Optional)
        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
