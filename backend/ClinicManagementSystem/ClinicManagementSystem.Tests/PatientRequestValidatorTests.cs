using ClinicManagementSystem.api.Contracts.Patient;
using FluentValidation.TestValidation;
using Xunit;

namespace ClinicManagementSystem.Tests;

/// <summary>
/// Bug Condition Exploration Tests - Task 1
/// These tests MUST FAIL on unfixed code to confirm the bug exists.
/// DO NOT attempt to fix when they fail - that's expected and correct.
/// </summary>
public class PatientRequestValidatorTests
{
    private readonly PatientRequestValidator _validator;

    public PatientRequestValidatorTests()
    {
        _validator = new PatientRequestValidator();
    }

    private static PatientRequest CreateValidPatientRequest(int gender)
    {
        return new PatientRequest(
            FirstName_En: "John",
            FirstName_Ar: "محمد",
            LastName_En: "Doe",
            LastName_Ar: "أحمد",
            DateOfBirth: DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            Gender: gender,
            Phone: "01012345678",
            Email: "test@example.com",
            Address_En: "123 Main St",
            Address_Ar: "شارع الرئيسي",
            EmergencyContactName: "Jane Doe",
            EmergencyContactPhone: "01098765432",
            Notes: "Test notes",
            IsActive: true
        );
    }

    /// <summary>
    /// Property 1: Bug Condition - Valid Gender Values Rejected
    /// EXPECTED: This test FAILS on unfixed code (bug exists)
    /// EXPECTED: This test PASSES after fix is applied
    /// </summary>
    [Fact]
    public void Validate_GenderMale_ShouldPass()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    /// <summary>
    /// Property 1: Bug Condition - Valid Gender Values Rejected
    /// EXPECTED: This test FAILS on unfixed code (bug exists)
    /// EXPECTED: This test PASSES after fix is applied
    /// </summary>
    [Fact]
    public void Validate_GenderFemale_ShouldPass()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 2);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    /// <summary>
    /// Property 2: Invalid Gender Values Should Be Rejected
    /// This should fail both before and after the fix (preservation)
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    [InlineData(999)]
    public void Validate_InvalidGender_ShouldFail(int invalidGender)
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: invalidGender);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender)
            .WithErrorMessage("Invalid gender value. Must be 1 (Male) or 2 (Female)");
    }

    /// <summary>
    /// Property 3: Preservation - Name Validations Unchanged
    /// Tests that all name validation rules continue to work as before
    /// </summary>
    [Fact]
    public void Validate_EmptyFirstNameEn_ShouldFail()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with { FirstName_En = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName_En)
            .WithErrorMessage("English first name is required");
    }

    [Fact]
    public void Validate_FirstNameEnTooLong_ShouldFail()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with 
        { 
            FirstName_En = new string('A', 101) 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName_En)
            .WithErrorMessage("English first name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_FirstNameEnWithNumbers_ShouldFail()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with { FirstName_En = "John123" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName_En)
            .WithErrorMessage("English first name must contain only English letters");
    }

    [Fact]
    public void Validate_EmptyFirstNameAr_ShouldFail()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with { FirstName_Ar = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName_Ar)
            .WithErrorMessage("Arabic first name is required");
    }

    /// <summary>
    /// Property 3: Preservation - Date of Birth Validations Unchanged
    /// </summary>
    [Fact]
    public void Validate_FutureDateOfBirth_ShouldFail()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with 
        { 
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Date of birth must be in the past");
    }

    [Fact]
    public void Validate_DateOfBirthTooOld_ShouldFail()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with 
        { 
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-151)) 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Invalid date of birth");
    }

    /// <summary>
    /// Property 3: Preservation - Phone Validations Unchanged
    /// </summary>
    [Fact]
    public void Validate_EmptyPhone_ShouldFail()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with { Phone = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Phone number is required");
    }

    [Theory]
    [InlineData("0201234567")] // Wrong prefix
    [InlineData("010123456")] // Too short
    [InlineData("01012345678901")] // Too long
    [InlineData("010ABCDEFGH")] // Contains letters
    public void Validate_InvalidPhoneFormat_ShouldFail(string invalidPhone)
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with { Phone = invalidPhone };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Invalid Egyptian phone number format. Must start with 010, 011, 012, or 015 followed by 8 digits");
    }

    /// <summary>
    /// Property 3: Preservation - Email Validations Unchanged
    /// </summary>
    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    public void Validate_InvalidEmailFormat_ShouldFail(string invalidEmail)
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with { Email = invalidEmail };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    /// <summary>
    /// Property 3: Preservation - Length Limit Validations Unchanged
    /// </summary>
    [Fact]
    public void Validate_AddressEnTooLong_ShouldFail()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with 
        { 
            Address_En = new string('A', 501) 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address_En)
            .WithErrorMessage("English address must not exceed 500 characters");
    }

    [Fact]
    public void Validate_NotesTooLong_ShouldFail()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1) with 
        { 
            Notes = new string('A', 2001) 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes must not exceed 2000 characters");
    }

    /// <summary>
    /// Integration test - Valid request with all fields
    /// </summary>
    [Fact]
    public void Validate_CompleteValidRequest_ShouldPass()
    {
        // Arrange
        var request = CreateValidPatientRequest(gender: 1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
