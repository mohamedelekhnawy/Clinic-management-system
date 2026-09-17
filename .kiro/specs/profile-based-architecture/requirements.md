# Requirements Document

## Introduction

This document specifies the requirements for refactoring the Clinic Management System from a role-based authentication architecture to a profile-based architecture. The current system has separate entity models (Doctor, Patient, Assistant) with duplicated identity information. The refactored system will consolidate identity into ApplicationUser with role-specific profile entities linked via one-to-one relationships. This architecture eliminates data duplication, simplifies authentication flows, and establishes a foundation for role-based authorization.

## Glossary

- **System**: The Clinic Management System backend API
- **ApplicationUser**: The core identity entity inheriting from IdentityUser, containing authentication credentials and basic user information
- **Profile_Entity**: Any role-specific entity (DoctorProfile, PatientProfile, AssistantProfile) containing specialized information for that role
- **Auth_Service**: The authentication service responsible for user registration, login, and token management
- **Database_Context**: The Entity Framework Core DbContext managing database operations
- **User_Manager**: ASP.NET Core Identity's UserManager for user account operations
- **Identity_Table**: The database table storing ApplicationUser records
- **Profile_Table**: Any database table storing role-specific profile records (DoctorProfiles, PatientProfiles, AssistantProfiles)
- **Foreign_Key**: A database column that references the primary key of another table
- **Navigation_Property**: An Entity Framework property that enables traversal between related entities
- **Migration**: An Entity Framework Core database schema version file
- **Service_Layer**: The application layer containing business logic services
- **Controller_Layer**: The API layer containing HTTP endpoint controllers
- **Registration_Flow**: The process of creating a new user account with associated profile
- **Authentication_Flow**: The process of validating credentials and issuing JWT tokens
- **JWT_Token**: JSON Web Token containing user claims for authorization
- **User_Claims**: Identity information embedded in JWT tokens (user ID, email, roles)
- **Cascading_Delete**: Database behavior where deleting a parent record automatically deletes related child records
- **Transactional_Operation**: A database operation that either fully succeeds or fully fails, maintaining data consistency

## Requirements

### Requirement 1: Profile Entity Model Creation

**User Story:** As a system architect, I want separate profile entities for each role, so that role-specific data is isolated while identity information remains centralized.

#### Acceptance Criteria

1. THE System SHALL create a DoctorProfile entity with properties: Id (int, primary key), UserId (string, foreign key to ApplicationUser.Id), ClinicId (int), Specialty_En, Specialty_Ar, Description_En, Description_Ar, Phone, SessionPrice, IsActive, and navigation properties for Clinic and ApplicationUser
2. THE System SHALL create a PatientProfile entity with properties: Id (int, primary key), UserId (string, foreign key to ApplicationUser.Id), DateOfBirth (DateOnly), Gender (enum), Phone, Email, Address_En, Address_Ar, EmergencyContactName, EmergencyContactPhone, Notes, IsActive, and navigation property for ApplicationUser
3. THE System SHALL create an AssistantProfile entity with properties: Id (int, primary key), UserId (string, foreign key to ApplicationUser.Id), ClinicId (int), Phone, IsActive, and navigation properties for Clinic and ApplicationUser
4. THE System SHALL inherit all profile entities from AuditableEntity to maintain CreatedAt, UpdatedAt, CreatedBy, and UpdatedBy tracking
5. THE System SHALL configure one-to-one relationships between ApplicationUser and each profile entity using Entity Framework Fluent API
6. THE System SHALL configure cascading delete behavior where deleting ApplicationUser deletes the associated profile
7. THE System SHALL enforce unique constraints on UserId foreign keys within each profile table to prevent multiple profiles of the same type per user

### Requirement 2: ApplicationUser Entity Enhancement

**User Story:** As a system architect, I want ApplicationUser to contain only identity-related information, so that authentication concerns are separated from role-specific data.

#### Acceptance Criteria

1. THE System SHALL add navigation properties to ApplicationUser: DoctorProfile (nullable), PatientProfile (nullable), and AssistantProfile (nullable)
2. THE System SHALL retain existing ApplicationUser properties: FirstName_EN, FirstName_AR, LastName_EN, LastName_AR, backward compatibility properties (FirstName, LastName), RefreshTokens collection, and email verification properties
3. THE System SHALL remove any role-specific properties that belong in profile entities from ApplicationUser
4. THE System SHALL configure Entity Framework relationships to enable lazy loading or explicit loading of profile entities
5. WHEN accessing a profile navigation property, THE System SHALL load the related profile data from the database

### Requirement 3: Database Migration Creation

**User Story:** As a database administrator, I want a migration that safely transforms the existing schema to the profile-based architecture, so that existing data is preserved during the transition.

#### Acceptance Criteria

1. THE System SHALL generate an Entity Framework Core migration named "RefactorToProfileBasedArchitecture"
2. THE Migration SHALL create three new tables: DoctorProfiles, PatientProfiles, and AssistantProfiles with appropriate columns, primary keys, and foreign key constraints
3. THE Migration SHALL create unique indexes on UserId columns in all profile tables
4. THE Migration SHALL create foreign key constraints from profile tables to AspNetUsers table with cascading delete behavior
5. THE Migration SHALL create indexes on ClinicId foreign keys in DoctorProfiles and AssistantProfiles tables
6. WHEN the migration is applied, THE System SHALL execute without errors on databases with existing data
7. THE Migration SHALL retain existing Doctor, Patient, and Assistant tables for data migration purposes
8. THE Migration SHALL NOT automatically migrate data (data migration will be handled separately)

### Requirement 4: Data Migration Strategy

**User Story:** As a system administrator, I want to migrate existing Doctor, Patient, and Assistant records to the new profile-based structure, so that no user data is lost during the refactoring.

#### Acceptance Criteria

1. THE System SHALL provide a data migration script that creates ApplicationUser records for each existing Doctor, Patient, and Assistant
2. WHEN creating ApplicationUser from Doctor records, THE Migration Script SHALL copy FirstName_En to FirstName_EN, FirstName_Ar to FirstName_AR, LastName_En to LastName_EN, LastName_Ar to LastName_AR, Email to Email property
3. WHEN creating ApplicationUser from Patient records, THE Migration Script SHALL copy FirstName_En to FirstName_EN, FirstName_Ar to FirstName_AR, LastName_En to LastName_EN, LastName_Ar to LastName_AR, Email to Email property where Email is not null
4. WHEN creating ApplicationUser from Assistant records, THE Migration Script SHALL copy FirstName_En to FirstName_EN, FirstName_Ar to FirstName_AR, LastName_En to LastName_EN, LastName_Ar to LastName_AR, Email to Email property
5. WHEN creating ApplicationUser records, THE Migration Script SHALL generate secure random passwords and set EmailConfirmed to true for backward compatibility
6. WHEN ApplicationUser is created, THE Migration Script SHALL create corresponding profile records (DoctorProfile, PatientProfile, or AssistantProfile) with UserId referencing the new ApplicationUser.Id
7. WHEN creating profile records, THE Migration Script SHALL copy all role-specific fields from the original entity to the profile entity
8. THE Migration Script SHALL maintain referential integrity by copying ClinicId references to profile tables
9. THE Migration Script SHALL execute within a database transaction to ensure atomic success or rollback
10. WHEN data migration completes successfully, THE Migration Script SHALL log the count of migrated users and profiles for each role
11. IF any migration step fails, THEN THE Migration Script SHALL roll back all changes and log detailed error information

### Requirement 5: Authentication Service Refactoring

**User Story:** As a backend developer, I want the authentication service to work with the profile-based architecture, so that registration and login operations create and validate users correctly.

#### Acceptance Criteria

1. WHEN a user registers with a role, THE Auth_Service SHALL create an ApplicationUser record with provided FirstName_EN, FirstName_AR, LastName_EN, LastName_AR, Email, and Password
2. WHEN ApplicationUser is created successfully, THE Auth_Service SHALL create the corresponding profile entity (DoctorProfile, PatientProfile, or AssistantProfile) with the UserId foreign key
3. WHEN creating a profile during registration, THE Auth_Service SHALL populate role-specific required fields with provided values or sensible defaults
4. WHEN registration includes ClinicId for Doctor or Assistant roles, THE Auth_Service SHALL assign ClinicId to the profile entity
5. THE Auth_Service SHALL wrap user creation and profile creation in a database transaction to ensure atomicity
6. IF profile creation fails after user creation, THEN THE Auth_Service SHALL roll back the transaction and return an appropriate error
7. WHEN a user logs in, THE Auth_Service SHALL authenticate against ApplicationUser credentials using UserManager
8. WHEN authentication succeeds, THE Auth_Service SHALL generate JWT tokens with claims including UserId from ApplicationUser.Id, Email, and assigned roles
9. WHEN generating tokens, THE Auth_Service SHALL include profile existence indicators in claims to enable role-based authorization
10. THE Auth_Service SHALL maintain existing email verification functionality with ApplicationUser email verification properties

### Requirement 6: User Service Refactoring

**User Story:** As a backend developer, I want user management services to query and update users through the profile-based architecture, so that user operations work seamlessly with the new structure.

#### Acceptance Criteria

1. WHEN querying users, THE User_Service SHALL load ApplicationUser records with their associated profile navigation properties
2. WHEN retrieving a doctor, THE User_Service SHALL query ApplicationUser records where DoctorProfile is not null and include DoctorProfile and related Clinic data
3. WHEN retrieving a patient, THE User_Service SHALL query ApplicationUser records where PatientProfile is not null and include PatientProfile data
4. WHEN retrieving an assistant, THE User_Service SHALL query ApplicationUser records where AssistantProfile is not null and include AssistantProfile and related Clinic data
5. WHEN updating user information, THE User_Service SHALL update ApplicationUser properties for identity fields (FirstName_EN, LastName_EN, etc.)
6. WHEN updating role-specific information, THE User_Service SHALL update the corresponding profile entity properties
7. WHEN deactivating a user, THE User_Service SHALL set IsActive to false on the profile entity rather than deleting records
8. WHEN deleting a user, THE User_Service SHALL delete the ApplicationUser record and rely on cascading delete to remove the associated profile
9. THE User_Service SHALL maintain transactional integrity when updating both ApplicationUser and profile entities in a single operation

### Requirement 7: Controller Layer Refactoring

**User Story:** As an API consumer, I want existing endpoints to continue working with minimal changes, so that client applications require minimal updates.

#### Acceptance Criteria

1. THE System SHALL maintain existing endpoint URLs for DoctorsController, PatientsController, and AssistantsController
2. WHEN receiving requests at existing endpoints, THE Controllers SHALL delegate to refactored services that use the profile-based architecture
3. WHEN returning user data, THE Controllers SHALL map ApplicationUser and profile entity data to existing response DTOs to maintain API contract compatibility
4. WHEN creating users through role-specific endpoints, THE Controllers SHALL invoke Auth_Service or User_Service methods that create both ApplicationUser and profile entities
5. WHEN updating users through role-specific endpoints, THE Controllers SHALL invoke service methods that update both ApplicationUser and profile entity properties as needed
6. THE Controllers SHALL return appropriate HTTP status codes (200, 201, 400, 404) consistent with existing behavior
7. THE Controllers SHALL maintain existing validation rules for request payloads using FluentValidation validators

### Requirement 8: Response DTO Mapping

**User Story:** As an API consumer, I want response structures to remain unchanged, so that my client application continues to work without modifications.

#### Acceptance Criteria

1. THE System SHALL maintain existing response DTO structures: DoctorResponse, PatientResponse, AssistantResponse
2. WHEN mapping ApplicationUser with DoctorProfile to DoctorResponse, THE System SHALL populate response properties from both ApplicationUser (FirstName_EN, LastName_EN, Email) and DoctorProfile (Specialty_En, Specialty_Ar, Description_En, Description_Ar, Phone, SessionPrice)
3. WHEN mapping ApplicationUser with PatientProfile to PatientResponse, THE System SHALL populate response properties from both ApplicationUser and PatientProfile
4. WHEN mapping ApplicationUser with AssistantProfile to AssistantResponse, THE System SHALL populate response properties from both ApplicationUser and AssistantProfile
5. THE System SHALL use AutoMapper or manual mapping to transform database entities to response DTOs
6. WHEN a profile entity has navigation properties loaded, THE System SHALL include related data in response DTOs (e.g., Clinic information for doctors)
7. THE System SHALL maintain existing JSON property naming conventions in API responses

### Requirement 9: Request DTO Validation

**User Story:** As a backend developer, I want request validation to enforce profile-based architecture rules, so that invalid data is rejected before processing.

#### Acceptance Criteria

1. WHEN creating a doctor, THE System SHALL validate that required DoctorProfile fields (Specialty_En, Phone, SessionPrice, ClinicId) are provided
2. WHEN creating a patient, THE System SHALL validate that required PatientProfile fields (DateOfBirth, Gender, Phone) are provided
3. WHEN creating an assistant, THE System SHALL validate that required AssistantProfile fields (Phone, ClinicId) are provided
4. WHEN registering any user, THE System SHALL validate that required ApplicationUser fields (FirstName_EN, LastName_EN, Email, Password) are provided
5. THE System SHALL validate email format using standard email validation rules
6. THE System SHALL validate password complexity according to ASP.NET Identity password requirements
7. WHEN validation fails, THE System SHALL return HTTP 400 Bad Request with detailed validation error messages
8. THE System SHALL use FluentValidation validators for request DTO validation

### Requirement 10: Database Relationship Configuration

**User Story:** As a database administrator, I want Entity Framework to correctly manage relationships between ApplicationUser and profile entities, so that data integrity is maintained.

#### Acceptance Criteria

1. THE Database_Context SHALL configure one-to-one relationship between ApplicationUser and DoctorProfile using HasOne and WithOne Fluent API methods
2. THE Database_Context SHALL configure one-to-one relationship between ApplicationUser and PatientProfile using HasOne and WithOne Fluent API methods
3. THE Database_Context SHALL configure one-to-one relationship between ApplicationUser and AssistantProfile using HasOne and WithOne Fluent API methods
4. THE Database_Context SHALL specify foreign key property UserId on each profile entity using HasForeignKey method
5. THE Database_Context SHALL configure OnDelete(DeleteBehavior.Cascade) for each profile relationship to enable cascading deletes
6. THE Database_Context SHALL configure IsRequired(false) on ApplicationUser navigation properties to allow users without profiles
7. THE Database_Context SHALL configure IsRequired(true) on profile entity navigation properties to enforce foreign key constraints
8. THE Database_Context SHALL configure unique indexes on UserId columns in profile tables using HasIndex method with IsUnique()

### Requirement 11: Dependency Injection Configuration

**User Story:** As a backend developer, I want dependency injection to provide access to refactored services, so that controllers and services can resolve dependencies correctly.

#### Acceptance Criteria

1. THE System SHALL register IAuthService with AuthService implementation in the DependencyInjection configuration
2. THE System SHALL register IUserService with UserService implementation in the DependencyInjection configuration
3. THE System SHALL register IDoctorService, IPatientService, and IAssistantService with corresponding implementations in the DependencyInjection configuration
4. THE System SHALL register AutoMapper profiles for entity-to-DTO mapping in the DependencyInjection configuration
5. THE System SHALL configure service lifetimes appropriately (Scoped for services that use DbContext, Singleton for stateless services)
6. WHEN controllers request services through constructor injection, THE System SHALL provide correctly configured service instances

### Requirement 12: Legacy Data Cleanup

**User Story:** As a system administrator, I want to safely remove old entity tables after confirming successful migration, so that the database schema reflects the new architecture.

#### Acceptance Criteria

1. THE System SHALL provide a cleanup migration that drops the old Doctor, Patient, and Assistant tables
2. THE Cleanup Migration SHALL only be applied after verifying successful data migration and system functionality
3. THE Cleanup Migration SHALL remove foreign key constraints referencing old tables before dropping tables
4. WHEN the cleanup migration is applied, THE System SHALL remove all references to old entity models from the codebase
5. THE System SHALL retain old migration files in the Migrations folder for historical reference and rollback capability

### Requirement 13: Error Handling and Logging

**User Story:** As a system administrator, I want comprehensive logging during migration and operation, so that I can diagnose issues and monitor system health.

#### Acceptance Criteria

1. WHEN creating ApplicationUser during registration, THE Auth_Service SHALL log user creation success or failure with user email
2. WHEN creating profile entities, THE Services SHALL log profile creation success or failure with profile type and user ID
3. WHEN database transactions fail, THE System SHALL log detailed error information including exception messages and stack traces
4. WHEN data migration executes, THE Migration Script SHALL log progress messages for each migration phase (user creation, profile creation, validation)
5. WHEN validation errors occur, THE System SHALL log validation failures with field names and error messages
6. THE System SHALL use structured logging with Serilog to enable log querying and analysis
7. WHEN cascading deletes execute, THE System SHALL log the deletion of ApplicationUser and associated profile records

### Requirement 14: Testing and Verification

**User Story:** As a QA engineer, I want to verify that the refactored system maintains existing functionality, so that users experience no disruption.

#### Acceptance Criteria

1. THE System SHALL pass all existing integration tests for authentication endpoints after refactoring
2. THE System SHALL pass all existing integration tests for user management endpoints after refactoring
3. WHEN running end-to-end tests, THE System SHALL successfully register users with all role types (Doctor, Patient, Assistant)
4. WHEN running end-to-end tests, THE System SHALL successfully authenticate registered users and return valid JWT tokens
5. WHEN running end-to-end tests, THE System SHALL successfully retrieve user profiles through role-specific endpoints
6. WHEN running end-to-end tests, THE System SHALL successfully update user information through role-specific endpoints
7. WHEN running end-to-end tests, THE System SHALL successfully delete users and verify cascading deletion of profiles
8. THE System SHALL include unit tests for service layer methods that create and query profile entities
9. THE System SHALL include unit tests for DTO mapping logic between entities and response models

### Requirement 15: Backward Compatibility

**User Story:** As a client application developer, I want API responses to maintain existing structure, so that my application continues to function without code changes.

#### Acceptance Criteria

1. THE System SHALL return doctor data with all properties that existed in the original Doctor entity response
2. THE System SHALL return patient data with all properties that existed in the original Patient entity response
3. THE System SHALL return assistant data with all properties that existed in the original Assistant entity response
4. WHEN bilingual fields are requested, THE System SHALL return both English (_En/_EN) and Arabic (_Ar/_AR) variants
5. THE System SHALL maintain existing JSON property casing conventions in API responses
6. THE System SHALL maintain existing error response formats with error codes and messages
7. THE System SHALL maintain existing pagination response structures for list endpoints
