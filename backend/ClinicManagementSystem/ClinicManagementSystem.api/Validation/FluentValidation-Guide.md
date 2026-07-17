# FluentValidation Guide

## Validation Convention

This project uses **FluentValidation** for all request validation.

### Rules

* Create a separate validator for each Request/DTO.
* Use `RuleFor()` for every property that requires validation.
* Always chain validation rules in a readable order.
* Every validation rule **must** include a meaningful `.WithMessage()`.
* Prefer built-in FluentValidation methods before writing custom validation.
* Keep validation logic inside validators only.
* Do **not** place validation logic inside Controllers, Services, or Handlers.
* Follow the same coding style throughout the project.

### Preferred Style

```csharp
RuleFor(x => x.Name)
    .NotEmpty()
    .MaximumLength(100)
    .WithMessage("Clinic name must not exceed 100 characters.");
```

---

## Common Validation Rules

> Add the reusable validation snippets below...


```csharp
using FluentValidation;

public class EntityValidator : AbstractValidator<Entity>
{
    public EntityValidator()
    {
        RuleFor(x => x.Property)
            .NotEmpty()
            .WithMessage("Property is required.");
    }
}
```

---

# Common Validation Rules

## Required String

```csharp
RuleFor(x => x.Name)
    .NotEmpty()
    .WithMessage("Name is required.");
```

---

## Maximum Length

```csharp
RuleFor(x => x.Name)
    .MaximumLength(100)
    .WithMessage("Name must not exceed 100 characters.");
```

---

## Minimum Length

```csharp
RuleFor(x => x.Password)
    .MinimumLength(8)
    .WithMessage("Password must be at least 8 characters.");
```

---

## Exact Length

```csharp
RuleFor(x => x.Code)
    .Length(6)
    .WithMessage("Code must be exactly 6 characters.");
```

---

## Email

```csharp
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()
    .WithMessage("Invalid email address.");
```

---

## Phone Number

```csharp
RuleFor(x => x.Phone)
    .NotEmpty()
    .Matches(@"^\+?[0-9]{10,15}$")
    .WithMessage("Invalid phone number.");
```

---

## Number Greater Than Zero

```csharp
RuleFor(x => x.Price)
    .GreaterThan(0)
    .WithMessage("Price must be greater than zero.");
```

---

## Number Greater Than or Equal

```csharp
RuleFor(x => x.Quantity)
    .GreaterThanOrEqualTo(0)
    .WithMessage("Quantity cannot be negative.");
```

---

## Range

```csharp
RuleFor(x => x.Age)
    .InclusiveBetween(18, 60)
    .WithMessage("Age must be between 18 and 60.");
```

---

## Date Required

```csharp
RuleFor(x => x.CreatedAt)
    .NotEqual(default(DateTime))
    .WithMessage("Date is required.");
```

---

## Compare Two Properties

```csharp
RuleFor(x => x.CloseTime)
    .GreaterThan(x => x.OpenTime)
    .WithMessage("Close time must be after open time.");
```

---

## Enum Validation

```csharp
RuleFor(x => x.Status)
    .IsInEnum()
    .WithMessage("Invalid status.");
```

---

## Guid Validation

```csharp
RuleFor(x => x.UserId)
    .NotEmpty()
    .WithMessage("UserId is required.");
```

---

## Nullable Property

```csharp
RuleFor(x => x.Description)
    .MaximumLength(500)
    .When(x => !string.IsNullOrWhiteSpace(x.Description));
```

---

## Regex

```csharp
RuleFor(x => x.Username)
    .Matches("^[a-zA-Z0-9_]+$")
    .WithMessage("Username contains invalid characters.");
```

---

## Custom Validation

```csharp
RuleFor(x => x.Age)
    .Must(age => age >= 18)
    .WithMessage("Age must be at least 18.");
```

---

## Conditional Validation

```csharp
RuleFor(x => x.CompanyName)
    .NotEmpty()
    .When(x => x.IsCompany);
```

---

## Validate Child Object

```csharp
RuleFor(x => x.Address)
    .SetValidator(new AddressValidator());
```

---

## Validate Collection

```csharp
RuleForEach(x => x.Items)
    .SetValidator(new ItemValidator());
```

---

# Common Chain Example

```csharp
RuleFor(x => x.Name)
    .NotEmpty()
    .MaximumLength(100)
    .WithMessage("Name must not exceed 100 characters.");
```

---

# Recommended Validation Order

1. `NotEmpty()`
2. `Length()` / `MaximumLength()` / `MinimumLength()`
3. `Matches()` / `EmailAddress()`
4. `GreaterThan()` / `LessThan()`
5. `Must()`
6. `When()`

---

# Best Practices

* Keep one Validator per entity or request (DTO).
* Use meaningful error messages.
* Validate DTOs instead of EF Core entities whenever possible.
* Reuse validators for nested objects using `SetValidator()`.
* Prefer `TimeOnly` for opening/closing hours instead of `DateTime`.
* Let the backend set `CreatedAt` and `UpdatedAt`; don't validate values that users never send.
