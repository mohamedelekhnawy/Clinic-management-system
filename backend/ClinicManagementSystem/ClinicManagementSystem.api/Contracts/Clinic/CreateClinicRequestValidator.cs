namespace ClinicManagementSystem.api.Contracts.Clinic
{
    public class CreateClinicRequestValidator :AbstractValidator<ClinicRequest>
    {
        public CreateClinicRequestValidator() {
            RuleFor(x => x.Name_En)
                .NotEmpty()
                .WithMessage("Clinic name (English) is required.")
                .MaximumLength(100)
                .WithMessage("Clinic name (English) must not exceed 100 characters.")
                .Matches(@"^[A-Za-z]+(?:[\s'\-][A-Za-z]+)*$")
                .WithMessage("Clinic name (English) must contain English letters only, with single spaces between words.");

            RuleFor(x => x.Name_Ar)
                .NotEmpty()
                .WithMessage("اسم العيادة بالعربي مطلوب.")
                .MaximumLength(100)
                .WithMessage("اسم العيادة بالعربي يجب ألا يتجاوز 100 حرف.")
                .Matches(@"^[\u0600-\u06FF]+(?:[\s\-][\u0600-\u06FF]+)*$")
                .WithMessage("اسم العيادة يجب أن يحتوي على حروف عربية فقط.");

            RuleFor(x => x.Address_En)
                .NotEmpty()
                .WithMessage("Clinic address (English) is required.")
                .MaximumLength(250)
                .WithMessage("Clinic address (English) must not exceed 250 characters.")
                .Matches(@"^[A-Za-z0-9][A-Za-z0-9\s,.\-#/()]*$")
                .WithMessage("Clinic address (English) contains invalid characters.");

            RuleFor(x => x.Address_Ar)
                .NotEmpty()
                .WithMessage("عنوان العيادة بالعربي مطلوب.")
                .MaximumLength(250)
                .WithMessage("عنوان العيادة بالعربي يجب ألا يتجاوز 250 حرف.")
                .Matches(@"^[\u0600-\u06FF0-9][\u0600-\u06FF0-9\s،.\-#/()]*$")
                .WithMessage("عنوان العيادة يحتوي على رموز غير مسموح بها.");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Clinic phone number is required.")
                .Matches(@"^01[0125][0-9]{8}$")
                .WithMessage("Phone number must be a valid Egyptian mobile number (e.g., 01012345678).");

            RuleFor(x => x.OpenTime)
                .NotEqual(default(DateTime))
                .WithMessage("Opening time is required.");

            RuleFor(x => x.CloseTime)
                .NotEqual(default(DateTime))
                .WithMessage("Closing time is required.")
                .GreaterThan(x => x.OpenTime)
                .WithMessage("Closing time must be later than opening time.");

        }
    }
}
