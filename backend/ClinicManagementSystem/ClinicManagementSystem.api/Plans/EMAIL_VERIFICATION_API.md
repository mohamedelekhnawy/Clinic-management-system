# Email Verification API Documentation

## Overview
This document describes the email verification feature implementation for the Clinic Management System API. The feature adds email verification to the user registration flow using 6-digit OTP codes sent via SMTP.

## Features
- ✅ Secure 6-digit OTP generation using `RandomNumberGenerator`
- ✅ Email verification required before login
- ✅ 15-minute OTP expiration
- ✅ 60-second rate limiting on resend requests
- ✅ Existing users automatically marked as verified (backward compatibility)
- ✅ HTML email templates with professional styling
- ✅ Comprehensive error handling

## Architecture Changes

### Database Schema
New columns added to `AspNetUsers` table:
- `EmailVerificationCode` (string, nullable) - Stores the 6-digit OTP
- `EmailVerificationCodeExpiresAt` (DateTime, nullable) - OTP expiration timestamp
- `IsEmailVerified` (bool, default: false) - Email verification status
- `EmailVerifiedAt` (DateTime, nullable) - Timestamp when email was verified
- `LastVerificationCodeSentAt` (DateTime, nullable) - Used for rate limiting

### Configuration
Add these settings to `appsettings.json` or environment variables:

```json
{
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-specific-password",
    "FromEmail": "noreply@clinicmanagement.com",
    "FromName": "Clinic Management System",
    "EnableSsl": true
  }
}
```

**Environment Variables (recommended for production):**
```bash
Email__Host=smtp.gmail.com
Email__Port=587
Email__Username=your-email@gmail.com
Email__Password=your-app-specific-password
Email__FromEmail=noreply@clinicmanagement.com
Email__FromName=Clinic Management System
Email__EnableSsl=true
```

## API Endpoints

### 1. Register User
**POST** `/auth/register`

Registers a new user and sends a verification email with a 6-digit OTP code.

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Success Response (200 OK):**
```json
{
  "message": "Registration successful. Please check your email for verification code."
}
```

**Error Responses:**
- `400 Bad Request` - Invalid input or email already exists
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Auth.EmailAlreadyExists",
  "status": 400,
  "detail": "Email is already registered"
}
```

**Notes:**
- OTP code expires in 15 minutes
- User cannot login until email is verified
- Email sent asynchronously (registration succeeds even if email fails)

---

### 2. Verify Email
**POST** `/auth/verify-email`

Verifies the user's email address using the 6-digit OTP code sent during registration.

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "code": "123456"
}
```

**Success Response (200 OK):**
```json
{
  "message": "Email verified successfully. You can now login."
}
```

**Error Responses:**

**Invalid Code (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Auth.InvalidVerificationCode",
  "status": 400,
  "detail": "Invalid verification code"
}
```

**Expired Code (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Auth.VerificationCodeExpired",
  "status": 400,
  "detail": "Verification code has expired. Please request a new one."
}
```

**Code Not Found (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Auth.VerificationCodeNotFound",
  "status": 400,
  "detail": "No verification code found. Please request a new verification code."
}
```

**Email Already Verified (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Auth.EmailAlreadyVerified",
  "status": 400,
  "detail": "Email is already verified"
}
```

**Validation Rules:**
- Code must be exactly 6 digits
- Code must contain only numeric characters
- Email must be valid format

---

### 3. Resend Verification Code
**POST** `/auth/resend-verification`

Generates and sends a new 6-digit OTP code to the user's email.

**Request Body:**
```json
{
  "email": "john.doe@example.com"
}
```

**Success Response (200 OK):**
```json
{
  "message": "Verification code sent. Please check your email."
}
```

**Error Responses:**

**Rate Limit Exceeded (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Auth.TooManyVerificationAttempts",
  "status": 400,
  "detail": "Too many requests. Please wait before requesting another verification code."
}
```

**Email Already Verified (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Auth.EmailAlreadyVerified",
  "status": 400,
  "detail": "Email is already verified"
}
```

**Rate Limiting:**
- Users can only request a new code once every **60 seconds**
- This is enforced via the `LastVerificationCodeSentAt` timestamp
- Rate limit is mandatory and cannot be bypassed

**Notes:**
- New code expires in 15 minutes
- Previous code is invalidated when new code is generated

---

### 4. Login
**POST** `/auth/login`

Authenticates user and returns JWT token. **Email must be verified** before login.

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Success Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "refreshToken": "aGF4ZGZhc2RmYXNkZmFzZGZhc2Rm...",
  "refreshTokenExpiration": "2026-09-11T18:00:00Z"
}
```

**Error Responses:**

**Email Not Verified (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Auth.EmailNotVerified",
  "status": 400,
  "detail": "Email address is not verified. Please check your email for verification code."
}
```

**Invalid Credentials (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Auth.InvalidCredentials",
  "status": 400,
  "detail": "Invalid email or password"
}
```

**Notes:**
- Existing users (before feature implementation) are automatically marked as verified
- Unverified users must complete email verification before logging in

---

## User Flows

### New User Registration Flow
```
1. User submits registration form
   POST /auth/register
   ↓
2. System creates user account (IsEmailVerified = false)
   ↓
3. System generates 6-digit OTP using RandomNumberGenerator
   ↓
4. System sends email with OTP code (expires in 15 minutes)
   ↓
5. User receives email and enters OTP code
   POST /auth/verify-email
   ↓
6. System validates OTP and marks email as verified
   ↓
7. User can now login
   POST /auth/login
```

### Resend Verification Code Flow
```
1. User clicks "Resend Code"
   POST /auth/resend-verification
   ↓
2. System checks rate limit (60-second cooldown)
   ↓
3. If allowed:
   - Generate new 6-digit OTP
   - Update expiration time (15 minutes)
   - Send new email
   - Update LastVerificationCodeSentAt
   ↓
4. If rate limited:
   - Return error "Too many requests"
```

### Existing Users (Backward Compatibility)
```
Existing users in database before feature deployment:
- IsEmailVerified = true (set by migration)
- EmailVerifiedAt = current timestamp
- Can login immediately without verification
```

---

## Error Codes Reference

| Error Code | Description | HTTP Status |
|-----------|-------------|-------------|
| `Auth.EmailNotVerified` | User attempted to login before verifying email | 400 |
| `Auth.InvalidVerificationCode` | OTP code doesn't match | 400 |
| `Auth.VerificationCodeExpired` | OTP code expired (>15 minutes) | 400 |
| `Auth.VerificationCodeNotFound` | No OTP code exists for user | 400 |
| `Auth.TooManyVerificationAttempts` | Rate limit exceeded (60-second cooldown) | 400 |
| `Auth.EmailAlreadyVerified` | Email is already verified | 400 |
| `Auth.EmailAlreadyExists` | Email already registered | 400 |
| `Auth.InvalidCredentials` | Wrong email or password | 400 |
| `Auth.UserNotFound` | User doesn't exist | 404 |

---

## Security Considerations

### OTP Generation
- Uses `System.Security.Cryptography.RandomNumberGenerator` for cryptographically secure random numbers
- Generates 6-digit codes (100000 - 999999)
- Code is stored in database (hashed would be better for production)

### Rate Limiting
- **Mandatory 60-second cooldown** between resend requests
- Prevents spam and abuse
- Tracked via `LastVerificationCodeSentAt` field

### Code Expiration
- All OTP codes expire after **15 minutes**
- Expired codes cannot be used
- User must request new code if expired

### Email Security
- SMTP credentials should be stored in environment variables or Azure Key Vault
- Enable SSL/TLS for email transmission
- Use app-specific passwords for Gmail (not account password)

---

## Testing

### Manual Testing with Swagger UI

1. **Start the application** with valid SMTP configuration
2. **Navigate to** `https://localhost:5001/swagger` (or your configured URL)
3. **Register a new user:**
   - Use `POST /auth/register`
   - Check email for 6-digit code
4. **Verify email:**
   - Use `POST /auth/verify-email`
   - Enter email and received code
5. **Test login:**
   - Use `POST /auth/login`
   - Should succeed after verification
6. **Test unverified login:**
   - Register another user
   - Try to login before verification
   - Should fail with `EmailNotVerified` error
7. **Test resend rate limiting:**
   - Call `POST /auth/resend-verification`
   - Call again immediately
   - Should fail with `TooManyVerificationAttempts` error
   - Wait 60 seconds and try again (should succeed)

### Postman/Thunder Client Collection

**Base URL:** `https://localhost:5001` (or your configured URL)

**Collection Variables:**
- `baseUrl`: Your API base URL
- `email`: Test user email
- `password`: Test user password
- `verificationCode`: Code received via email

**Test Scenarios:**

1. **Happy Path - New User Registration**
```
POST {{baseUrl}}/auth/register
Body: { "firstName": "Test", "lastName": "User", "email": "{{email}}", "password": "{{password}}" }
Expected: 200 OK with message
Action: Check email for code, save to {{verificationCode}}
```

2. **Verify Email**
```
POST {{baseUrl}}/auth/verify-email
Body: { "email": "{{email}}", "code": "{{verificationCode}}" }
Expected: 200 OK with success message
```

3. **Login After Verification**
```
POST {{baseUrl}}/auth/login
Body: { "email": "{{email}}", "password": "{{password}}" }
Expected: 200 OK with JWT token
```

4. **Login Before Verification (Error Case)**
```
POST {{baseUrl}}/auth/register (new email)
POST {{baseUrl}}/auth/login (same email, before verification)
Expected: 400 Bad Request - EmailNotVerified
```

5. **Rate Limiting Test**
```
POST {{baseUrl}}/auth/resend-verification
Body: { "email": "{{email}}" }
Expected: 200 OK

POST {{baseUrl}}/auth/resend-verification (immediately)
Body: { "email": "{{email}}" }
Expected: 400 Bad Request - TooManyVerificationAttempts
```

6. **Expired Code Test**
```
Wait 15+ minutes after registration
POST {{baseUrl}}/auth/verify-email
Body: { "email": "{{email}}", "code": "{{verificationCode}}" }
Expected: 400 Bad Request - VerificationCodeExpired
```

---

## Troubleshooting

### Email Not Sending

**Problem:** User registered but didn't receive verification email

**Possible Causes:**
1. SMTP credentials not configured
2. SMTP host/port incorrect
3. App-specific password required (Gmail)
4. Firewall blocking SMTP port
5. Email in spam folder

**Solutions:**
1. Check `appsettings.json` or environment variables for Email configuration
2. Verify SMTP settings with email provider documentation
3. For Gmail: Enable 2FA and create app-specific password
4. Check application logs for email sending errors
5. Ask user to check spam/junk folder

### Rate Limiting Issues

**Problem:** User can't resend code even after waiting

**Possible Causes:**
1. Server time zone mismatch
2. `LastVerificationCodeSentAt` not being updated
3. 60-second calculation error

**Solutions:**
1. Ensure server uses UTC time (`DateTime.UtcNow`)
2. Check database for `LastVerificationCodeSentAt` value
3. Review `ResendVerificationCodeAsync` implementation

### Verification Code Not Working

**Problem:** User enters correct code but verification fails

**Possible Causes:**
1. Code expired (>15 minutes)
2. Code comparison case-sensitive
3. Code regenerated (old code invalidated)

**Solutions:**
1. Check `EmailVerificationCodeExpiresAt` in database
2. Code comparison uses `StringComparison.Ordinal` (case-sensitive)
3. Latest code in database is the valid one

---

## Production Deployment Checklist

### Configuration
- [ ] SMTP credentials stored in Azure Key Vault or secure secrets manager
- [ ] Email configuration tested with production SMTP server
- [ ] Rate limiting settings reviewed (60-second cooldown appropriate?)
- [ ] OTP expiration time reviewed (15 minutes appropriate?)

### Database
- [ ] Migration applied to production database
- [ ] Existing users verified in database (`IsEmailVerified = true`)
- [ ] Backup taken before migration

### Monitoring
- [ ] Logging configured for email verification events
- [ ] Alerts set up for email sending failures
- [ ] Metrics tracking verification success rate
- [ ] Rate limiting abuse detection

### Security
- [ ] SMTP uses SSL/TLS
- [ ] Credentials not exposed in logs
- [ ] OTP codes not logged in plaintext
- [ ] Rate limiting enforced and tested

### Testing
- [ ] End-to-end test with production SMTP
- [ ] Rate limiting tested in production-like environment
- [ ] Email templates render correctly in major email clients
- [ ] Unverified login blocked in production

---

## Future Enhancements

### Potential Improvements
1. **Hash OTP codes** in database instead of storing plaintext
2. **Add attempt counter** to block after X failed verification attempts
3. **Support email template customization** via admin panel
4. **Add phone verification** as alternative to email
5. **Implement "magic link"** as alternative to OTP code
6. **Add email change verification** workflow
7. **Support multiple email providers** (SendGrid, AWS SES, etc.)
8. **Add verification reminder emails** if not verified after 24 hours
9. **Implement account cleanup** for unverified accounts after X days
10. **Add metrics dashboard** for verification analytics

---

## Support

For issues or questions about the email verification feature:
1. Check application logs for detailed error messages
2. Review this documentation for troubleshooting steps
3. Verify SMTP configuration is correct
4. Test email sending manually with SMTP credentials
5. Contact development team with specific error codes and logs

---

## Change Log

### Version 1.0.0 (2026-09-04)
- Initial implementation of email verification feature
- 6-digit OTP using RandomNumberGenerator
- 60-second mandatory rate limiting
- 15-minute OTP expiration
- HTML email templates
- Backward compatibility for existing users
- Comprehensive API documentation
