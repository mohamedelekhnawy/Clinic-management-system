# Security Configuration Guide

## Development Environment

### User Secrets (Local Development)
```bash
# Initialize user secrets (already done)
dotnet user-secrets init

# Set JWT Key (MUST be at least 32 characters)
dotnet user-secrets set "Jwt:Key" "YourSuperSecretKeyForJWTAuthenticationThatShouldBeAtLeast32CharactersLong"

# Set JWT Configuration (Optional - defaults in appsettings.Development.json)
dotnet user-secrets set "Jwt:Issuer" "ClinicManagementSystem"
dotnet user-secrets set "Jwt:Audience" "ClinicManagementSystemUsers"
dotnet user-secrets set "Jwt:ExpirationInMinutes" "60"

# Set Database Connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=ClinicManagementSystem;TrustServerCertificate=True;Trusted_Connection=True;Encrypt=False;"

# List all secrets
dotnet user-secrets list
```

## JWT Configuration Validation

The application validates JWT settings at startup with **DataAnnotations** and **Runtime Checks**:

### Validation Rules:
- ✅ **Jwt:Key** - REQUIRED, minimum 32 characters
- ✅ **Jwt:Issuer** - REQUIRED
- ✅ **Jwt:Audience** - REQUIRED
- ✅ **Jwt:ExpirationInMinutes** - Must be between 1 and 10080 (7 days)

### Validation Modes:
1. **Data Annotations** - Validates on Options binding
2. **ValidateOnStart** - Fails application startup if invalid
3. **Runtime Validation** - Additional checks with clear error messages

### Example Error Messages:
```
InvalidOperationException: JWT Key is not configured. Please set it in User Secrets or Environment Variables.
InvalidOperationException: JWT Key must be at least 32 characters long.
InvalidOperationException: JWT ExpirationInMinutes must be between 1 and 10080 (7 days).
```

## Production Environment

### Environment Variables
Set these environment variables in your production environment:

```bash
Jwt__Key=<Generate-Strong-Random-Key-Here-MinLength32>
Jwt__Issuer=ClinicManagementSystem
Jwt__Audience=ClinicManagementSystemUsers
Jwt__ExpirationInMinutes=60
ConnectionStrings__DefaultConnection=<Your-Production-DB-Connection>
```

### Azure App Service
1. Go to: App Service > Configuration > Application Settings
2. Add:
   - `Jwt__Key` = [Strong Random Key - Min 32 chars]
   - `Jwt__Issuer` = ClinicManagementSystem
   - `Jwt__Audience` = ClinicManagementSystemUsers
   - `Jwt__ExpirationInMinutes` = 60
   - `ConnectionStrings__DefaultConnection` = [Production DB]

### Docker
```yaml
# docker-compose.yml
environment:
  - Jwt__Key=${JWT_KEY}
  - Jwt__Issuer=ClinicManagementSystem
  - Jwt__Audience=ClinicManagementSystemUsers
  - Jwt__ExpirationInMinutes=60
  - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
```

### Kubernetes
```yaml
# secrets.yaml
apiVersion: v1
kind: Secret
metadata:
  name: clinic-secrets
type: Opaque
data:
  jwt-key: <base64-encoded-key-min-32-chars>
  jwt-issuer: <base64-encoded-issuer>
  jwt-audience: <base64-encoded-audience>
  db-connection: <base64-encoded-connection>
```

### IIS
1. Server Manager > Configuration Editor
2. system.webServer/aspNetCore/environmentVariables
3. Add variables

## Generate Strong JWT Key

```bash
# PowerShell (Generates 64 character key)
$bytes = New-Object byte[] 64
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

```bash
# Linux/Mac (Generates 64 character key)
openssl rand -base64 64
```

## Security Checklist

- ✅ JWT Key stored in User Secrets (Development)
- ✅ JWT Key removed from appsettings.json
- ✅ JWT Key validation (min 32 characters)
- ✅ Startup validation with clear error messages
- ✅ DataAnnotations on JwtOptions
- ✅ ValidateOnStart enabled
- ✅ ConnectionString should be in secrets/environment variables
- ✅ appsettings.json committed to git (no secrets)
- ✅ appsettings.Development.json can be committed
- ✅ appsettings.Production.json committed (no secrets)
- ⚠️ Never commit .env files with real secrets
- ⚠️ Use different JWT keys for each environment
- ⚠️ Rotate JWT keys periodically in production
- ⚠️ Ensure JWT Key is at least 32 characters in all environments
