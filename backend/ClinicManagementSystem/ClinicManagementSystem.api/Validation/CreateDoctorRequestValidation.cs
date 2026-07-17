namespace ClinicManagementSystem.api.Validation
{
    public class CreateDoctorRequestValidation:AbstractValidator<DoctorRequest>
    {
        public CreateDoctorRequestValidation() 
        {
            RuleFor(x => x.FirstName_En)
                .NotEmpty()
                .WithMessage("First name (English) is required.")
                .MaximumLength(100)
                .WithMessage("First name (English) must not exceed 100 characters.")
                .Matches(@"^[A-Za-z]+(?:[\s'\-][A-Za-z]+)*$")
                .WithMessage("First name (English) must contain English letters only, with single spaces between words.");

            RuleFor(x => x.FirstName_Ar)
                .NotEmpty()
                .WithMessage("First name (Arabic) is required.")
                .MaximumLength(100)
                .WithMessage("First name (Arabic) must not exceed 100 characters.")
                .Matches(@"^[\u0600-\u06FF]+(?:\s[\u0600-\u06FF]+)*$")
                .WithMessage("First name (Arabic) must contain Arabic letters only.");

            RuleFor(x => x.LastName_En)
                .NotEmpty()
                .WithMessage("Last name (English) is required.")
                .MaximumLength(100)
                .WithMessage("Last name (English) must not exceed 100 characters.")
                .Matches(@"^[A-Za-z]+(?:[\s'\-][A-Za-z]+)*$")
                .WithMessage("Last name (English) must contain English letters only, with single spaces between words.");

            RuleFor(x => x.LastName_Ar)
                .NotEmpty()
                .WithMessage("Last name (Arabic) is required.")
                .MaximumLength(100)
                .WithMessage("Last name (Arabic) must not exceed 100 characters.")
                .Matches(@"^[\u0600-\u06FF]+(?:\s[\u0600-\u06FF]+)*$")
                .WithMessage("Last name (Arabic) must contain Arabic letters only.");

            RuleFor(x => x.Specialty_En)
                .NotEmpty()
                .WithMessage("Specialty (English) is required.")
                .MaximumLength(150)
                .WithMessage("Specialty (English) must not exceed 150 characters.")
                .Matches(@"^[A-Za-z]+(?:[\s'\-][A-Za-z]+)*$")
                .WithMessage("Specialty (English) must contain English letters only.");

            RuleFor(x => x.Specialty_Ar)
                .NotEmpty()
                .WithMessage("Specialty (Arabic) is required.")
                .MaximumLength(150)
                .WithMessage("Specialty (Arabic) must not exceed 150 characters.")
                .Matches(@"^[\u0600-\u06FF]+(?:\s[\u0600-\u06FF]+)*$")
                .WithMessage("Specialty (Arabic) must contain Arabic letters only.");

            RuleFor(x => x.Description_En)
                .MaximumLength(1000)
                .WithMessage("Description (English) must not exceed 1000 characters.");

            RuleFor(x => x.Description_Ar)
                .MaximumLength(1000)
                .WithMessage("Description (Arabic) must not exceed 1000 characters.");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .MaximumLength(20)
                .WithMessage("Phone number must not exceed 20 characters.")
                .Matches(@"^\+?[0-9]{7,20}$")
                .WithMessage("Phone number must contain only digits and may start with '+'.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .MaximumLength(255)
                .WithMessage("Email must not exceed 255 characters.")
                .EmailAddress()
                .WithMessage("Please enter a valid email address.");

            RuleFor(x => x.SessionPrice)
                .NotEmpty()
                .WithMessage("Session price is required.")
                .GreaterThan(0)
                .WithMessage("Session price must be greater than zero.");
        }
    }
}
