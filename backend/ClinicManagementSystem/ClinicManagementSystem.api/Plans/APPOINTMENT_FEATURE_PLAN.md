# Appointment Feature - Implementation Plan (REVISED)

## 📋 Overview

This document defines the complete implementation plan for the Appointments system in the Clinic Management System.

**Important:** This is a revised plan based on business requirements analysis. The plan focuses on maintainability, business rule clarity, and alignment with the existing project architecture.

**DO NOT start implementing until this plan is reviewed and approved.**

---

## 🔄 Naming Updates

Based on the current project codebase, the following naming conventions are established:

### Current Project Names (Source of Truth):
- **DbSet Names:**
  - `Clinics` (plural)
  - `Doctor` (singular) ⚠️
  - `Patients` (plural)
  - `Assistants` (plural)
  - `Appointments` (plural) ← **TO BE ADDED**

- **Navigation Properties:**
  - `Doctor.clinic` (lowercase) ⚠️
  - `Appointment.Patient` (PascalCase)
  - `Appointment.Doctor` (PascalCase)

- **Auditable Fields Convention:**
  - `CreatedOn` (DateTime)
  - `CreatedBy` (string?)
  - `UpdatedOn` (DateTime?)
  - `UpdatedBy` (string?)

- **Error Types:**
  - `ErrorType.Failure`
  - `ErrorType.Validation`
  - `ErrorType.NotFound`
  - `ErrorType.Conflict`

### ⚠️ Inconsistencies Noted:
The project has naming inconsistencies:
- DbSet `Doctor` is singular while others are plural
- Navigation property `clinic` is lowercase while others are PascalCase

**Decision:** Follow the **current** naming as-is to maintain consistency with existing code. Do NOT "fix" these during Appointment implementation.

---

## 🏗️ Architecture Pattern

The project follows Clean Architecture with these patterns:

### 1. **Clean Architecture**
```
Controllers → Services → Persistence (DbContext)
     ↓           ↓
  Contracts   Result Pattern
     ↓
  Validators (FluentValidation)
```

### 2. **Result Pattern for Error Handling**
- Use `Result<T>` and `Result` instead of throwing exceptions
- Define errors in `*Errors.cs` files in `Abstractions` folder
- Error types: `Failure`, `Validation`, `NotFound`, `Conflict`
- Controllers use RFC 7807 Problem Details via extension method

### 3. **Request/Response Pattern**
- Request DTOs for input
- Response DTOs for output
- FluentValidation for input validation
- Mapster for object mapping

### 4. **Service Layer Pattern**
- Interface + Implementation
- Service contains ALL business logic
- Thin controllers (only routing, mapping, status codes)

### 5. **Authorization**
- JWT-based authentication
- Role-based authorization using `[Authorize]` attributes
- Roles checked via ClaimsPrincipal

### 6. **Auditable Entities**
- All domain entities inherit from `AuditableEntity`
- Audit fields automatically populated in `SaveChangesAsync`

---

## 📦 Appointment Model

### Current Model (Already Exists)
```csharp
public class Appointment : AuditableEntity
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
}
```

### ✅ Design Decisions:
- **No ClinicId:** Clinic is accessed via `Appointment → Doctor → Clinic`
- **No AssistantId:** Assistant relationship not needed in the model
- **No QueuePosition:** Calculated dynamically, not stored
- **No EstimatedStartTime:** Calculated dynamically
- **Inherits AuditableEntity:** Provides `CreatedOn`, `CreatedBy`, `UpdatedOn`, `UpdatedBy`

---

## 📊 Appointment Statuses

### Enum Definition (Already Exists)
```csharp
public enum AppointmentStatus
{
    Scheduled = 1,    // Initial booking
    Confirmed = 2,    // Confirmed by clinic/patient
    Waiting = 3,      // Patient checked in
    InProgress = 4,   // Doctor started consultation
    Completed = 5,    // Consultation finished
    Cancelled = 6,    // Cancelled before completion
    NoShow = 7        // Patient did not show up
}
```

### Valid Status Transitions

| From         | To                                      | Business Rule                           |
|--------------|-----------------------------------------|-----------------------------------------|
| Scheduled    | Confirmed, Waiting, Cancelled, NoShow   | Normal flow                             |
| Confirmed    | Waiting, Cancelled, NoShow              | Normal flow                             |
| Waiting      | InProgress, Cancelled, NoShow           | Requires doctor arrival                 |
| InProgress   | Completed, Cancelled                    | Cannot go back to Waiting               |
| Completed    | *(none)*                                | Final state                             |
| Cancelled    | *(none)*                                | Final state                             |
| NoShow       | *(none)*                                | Final state                             |

### ❌ Invalid Transitions (Must Be Prevented)
- Completed → Waiting
- Completed → InProgress
- Completed → Scheduled
- Cancelled → InProgress
- Cancelled → Waiting
- NoShow → any other status
- InProgress → Scheduled

---

## 📋 Business Rules

### Core Business Rules

1. **Patient Rules:**
   - Patient can have multiple appointments
   - Patient **CANNOT** have overlapping appointments
   - Patient can only view/manage their own appointments (unless Admin/Assistant)

2. **Doctor Rules:**
   - Doctor **CANNOT** have overlapping appointments
   - Doctor must have a working schedule defining available hours
   - Appointments must fall within doctor's schedule
   - Doctor can only start appointments after arriving at clinic

3. **Appointment Timing:**
   - Appointment **CANNOT** be created in the past
   - `EndTime` **MUST** be after `StartTime`
   - Appointment date + time must fall within doctor's working hours

4. **Check-In Rules:**
   - Check-in **DOES NOT** require doctor arrival
   - Patient/Assistant can check in before doctor arrives
   - After check-in: `Status = Waiting`, `CheckedInAt = DateTime.UtcNow`
   - Can only check in appointments with status `Scheduled` or `Confirmed`

5. **Start Appointment Rules:**
   - Can only start appointments with status `Waiting`
   - **REQUIRES** doctor to have arrived/checked in at clinic
   - Sets `Status = InProgress`, `ActualStartTime = DateTime.UtcNow`

6. **Complete Appointment Rules:**
   - Can only complete appointments with status `InProgress`
   - Sets `Status = Completed`, `ActualEndTime = DateTime.UtcNow`

7. **Cancellation Rules:**
   - **CANNOT** cancel `Completed` appointments
   - Can cancel `Scheduled`, `Confirmed`, `Waiting`, or `InProgress` appointments
   - Cancellation is the **preferred** operation instead of DELETE

8. **Update Rules:**
   - Can update `Scheduled` appointments freely
   - Can update `Confirmed` appointments with restrictions
   - **CANNOT** update `Waiting`, `InProgress`, `Completed`, `Cancelled`, or `NoShow`
   - All booking validations must re-run on update

9. **Delete Rules:**
   - Physical deletion is **NOT** a normal business operation
   - Appointments are historical records
   - Use cancellation instead
   - If DELETE is kept, only allow deleting `Scheduled` or `Cancelled` appointments

10. **Queue Rules:**
    - Queue position is calculated dynamically (not stored)
    - Ordered by `CheckedInAt` timestamp
    - Current patient = `InProgress` status
    - Waiting patients = `Waiting` status
    - Estimated wait time is calculated, not guaranteed

---

## 🔒 Authorization Rules

### Roles
- **Patient:** Can book and view their own appointments
- **Assistant:** Can manage appointments for the clinic
- **Doctor:** Can manage their clinical workflow
- **Admin:** Full access

### Operation Authorization Matrix

| Operation                  | Patient (Own) | Assistant | Doctor (Own) | Admin |
|----------------------------|---------------|-----------|--------------|-------|
| GET /api/Appointments      | ❌ No         | ✅ Yes    | ✅ Yes       | ✅ Yes |
| GET /api/Appointments/{id} | ✅ Own only   | ✅ Yes    | ✅ Yes       | ✅ Yes |
| POST /api/Appointments     | ✅ Own only   | ✅ Yes    | ❌ No        | ✅ Yes |
| PUT /api/Appointments/{id} | ✅ Own only   | ✅ Yes    | ❌ No        | ✅ Yes |
| DELETE (if kept)           | ❌ No         | ⚠️ Restricted | ❌ No    | ⚠️ Restricted |
| POST .../check-in          | ❌ No         | ✅ Yes    | ❌ No        | ✅ Yes |
| POST .../start             | ❌ No         | ❌ No     | ✅ Yes       | ✅ Yes |
| POST .../complete          | ❌ No         | ❌ No     | ✅ Yes       | ✅ Yes |
| POST .../cancel            | ✅ Own only   | ✅ Yes    | ✅ Yes       | ✅ Yes |
| GET .../doctor/{id}/queue  | ❌ No         | ✅ Yes    | ✅ Own only  | ✅ Yes |

### Critical Authorization Requirements:
1. **Patient MUST NOT access all appointments via GET /api/Appointments**
2. **Service layer MUST enforce ownership checks**
3. **Doctor can only view their own queue**
4. **Assistant can only manage appointments for their clinic** (future: multi-clinic support)

---

## ✅ Validation Rules

### Creation Validations (POST)

#### Input Validation (FluentValidation)
- `PatientId` > 0
- `DoctorId` > 0
- `AppointmentDate` >= Today
- `StartTime` is required
- `EndTime` is required
- `EndTime` > `StartTime`
- `Notes` <= 1000 characters

#### Business Validation (Service Layer)
1. ✅ Patient exists
2. ✅ Doctor exists
3. ✅ Appointment is not in the past
4. ✅ End time > Start time
5. ✅ Doctor schedule allows the appointment *(Phase 5)*
6. ✅ Doctor has no conflicting appointment
7. ✅ Patient has no conflicting appointment
8. ✅ Status defaults to `Scheduled`

#### Conflict Detection Logic
**Doctor Conflict:**
```sql
WHERE DoctorId = @doctorId
  AND AppointmentDate = @date
  AND Status NOT IN (Cancelled, NoShow)
  AND (StartTime < @newEndTime AND EndTime > @newStartTime)
```

**Patient Conflict:**
```sql
WHERE PatientId = @patientId
  AND AppointmentDate = @date
  AND Status NOT IN (Cancelled, NoShow)
  AND (StartTime < @newEndTime AND EndTime > @newStartTime)
```

### Update Validations (PUT)

1. ✅ Appointment exists
2. ✅ Status allows updates:
   - `Scheduled` → ✅ Allowed
   - `Confirmed` → ✅ Allowed
   - `Waiting` → ❌ Not allowed
   - `InProgress` → ❌ Not allowed
   - `Completed` → ❌ Not allowed
   - `Cancelled` → ❌ Not allowed
   - `NoShow` → ❌ Not allowed
3. ✅ All creation validations re-run
4. ✅ Conflict queries **MUST exclude current appointment ID**

---

## 🔄 Appointment Lifecycle

### Lifecycle Flow Diagram
```
   Scheduled ──────┐
      │            │
      ▼            │
   Confirmed       │
      │            │
      ▼            ▼
   [Check-In] → Waiting ────┐
                   │         │
    [Doctor        ▼         │
     Arrives]   InProgress   │
                   │         │
                   ▼         ▼
               Completed   Cancelled
                             │
                             ▼
                          NoShow
```

### Actions & Transitions

#### 1. Check-In (POST /api/Appointments/{id}/check-in)
- **Allowed From:** `Scheduled`, `Confirmed`
- **Transition To:** `Waiting`
- **Sets:** `CheckedInAt = DateTime.UtcNow`
- **Authorization:** Assistant, Admin
- **Business Rule:** Does NOT require doctor arrival

#### 2. Start (POST /api/Appointments/{id}/start)
- **Allowed From:** `Waiting`
- **Transition To:** `InProgress`
- **Sets:** `ActualStartTime = DateTime.UtcNow`
- **Authorization:** Doctor (own), Admin
- **Business Rule:** REQUIRES doctor to have arrived/checked in

#### 3. Complete (POST /api/Appointments/{id}/complete)
- **Allowed From:** `InProgress`
- **Transition To:** `Completed`
- **Sets:** `ActualEndTime = DateTime.UtcNow`
- **Authorization:** Doctor (own), Admin

#### 4. Cancel (POST /api/Appointments/{id}/cancel)
- **Allowed From:** `Scheduled`, `Confirmed`, `Waiting`, `InProgress`
- **Transition To:** `Cancelled`
- **Authorization:** Patient (own), Assistant, Doctor, Admin
- **Business Rule:** Cannot cancel `Completed` appointments

#### 5. NoShow (Future consideration)
- **Allowed From:** `Scheduled`, `Confirmed`
- **Transition To:** `NoShow`
- **Authorization:** Assistant, Doctor, Admin
- **Triggered:** When patient doesn't show up and time passes

---

## 🩺 Doctor Schedule (Dependency)

### Why Needed
Appointments must be validated against doctor's working hours.

### Conceptual Model
```csharp
public class DoctorSchedule : AuditableEntity
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation
    public Doctor Doctor { get; set; } = null!;
}
```

### Business Rules
- Doctor can have multiple schedules (different days)
- One schedule per day of week per doctor
- Appointment validation must check:
  - `AppointmentDate.DayOfWeek` matches schedule
  - `AppointmentTime` falls within `StartTime` and `EndTime`

### Example Validation
```
Doctor Schedule:
  Monday: 10:00 → 15:00

Appointment Request:
  Date: Monday, 2026-08-18
  Time: 16:00 → 16:30
  
Result: REJECT (outside working hours)
```

### Implementation Priority
**Phase 5** - Required before full appointment booking validation

### Related Endpoints (Future)
```
GET    /api/DoctorSchedules
GET    /api/DoctorSchedules/{id}
POST   /api/DoctorSchedules
PUT    /api/DoctorSchedules/{id}
DELETE /api/DoctorSchedules/{id}
GET    /api/Doctors/{doctorId}/schedule
```

---

## 👨‍⚕️ Doctor Attendance/Arrival (Dependency)

### Why Needed
- Doctor must physically arrive at clinic before starting appointments
- Patients can check in and wait even if doctor hasn't arrived yet
- System must track doctor arrival to control appointment start

### Conceptual Model
```csharp
public class DoctorAttendance : AuditableEntity
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? ExpectedArrivalTime { get; set; }
    public DateTime? ActualArrivalTime { get; set; }
    public DateTime? ActualDepartureTime { get; set; }
    
    // Navigation
    public Doctor Doctor { get; set; } = null!;
}
```

### Business Rules
- One attendance record per doctor per day
- Doctor marks arrival by checking in (sets `ActualArrivalTime`)
- Appointment service checks arrival before allowing `Waiting → InProgress`
- If doctor hasn't arrived: appointments stay in `Waiting` status

### Example Scenario
```
10:00 - Patient checks in → Status = Waiting
10:15 - Doctor arrives → ActualArrivalTime = 10:15
10:16 - Doctor starts appointment → Status = InProgress
```

### Validation Logic in AppointmentService.StartAppointmentAsync
```csharp
// Check if doctor has arrived today
var doctorArrived = await _context.DoctorAttendances
    .AnyAsync(da => 
        da.DoctorId == appointment.DoctorId &&
        da.Date == DateOnly.FromDateTime(DateTime.Today) &&
        da.ActualArrivalTime.HasValue,
        cancellationToken);

if (!doctorArrived)
    return Result.Failure(AppointmentErrors.DoctorNotArrived);
```

### Implementation Priority
**Phase 6** - Required before allowing appointment start

### Related Endpoints (Future)
```
POST   /api/DoctorAttendance/check-in          (Doctor marks arrival)
POST   /api/DoctorAttendance/check-out         (Doctor marks departure)
GET    /api/DoctorAttendance/today             (Get today's attendance)
GET    /api/DoctorAttendance/doctor/{id}/today
```

---

## 🚶‍♂️ Queue Management

### Queue Concept
Queue is a **dynamic view**, not stored data.

### Queue Components

#### 1. Current Patient (1 or 0)
- Status: `InProgress`
- The patient currently being seen by the doctor

#### 2. Waiting Patients (0 to N)
- Status: `Waiting`
- Ordered by: `CheckedInAt ASC`
- Position calculated dynamically

#### 3. Upcoming Appointments (optional in queue view)
- Status: `Scheduled` or `Confirmed`
- Not yet checked in

### Queue Position Calculation
```csharp
var waitingPatients = await _context.Appointments
    .Where(a => a.DoctorId == doctorId &&
                a.AppointmentDate == date &&
                a.Status == AppointmentStatus.Waiting)
    .OrderBy(a => a.CheckedInAt)
    .Select((a, index) => new { Appointment = a, Position = index + 1 })
    .ToListAsync(cancellationToken);
```

### Estimated Waiting Time
- **NOT** a stored field
- Calculated dynamically
- Formula (basic version):
  ```
  EstimatedMinutes = (WaitingCount * DefaultConsultationDuration) + RemainingTimeForCurrent
  ```
- Default consultation duration: **30 minutes** (configurable)
- This is an **estimate**, not a guarantee
- Future improvement: use historical average duration per doctor

### Important Notes
- ❌ Do NOT store `QueuePosition` in Appointment table
- ❌ Do NOT store `EstimatedWaitingTime` in Appointment table
- ✅ Calculate these values dynamically when requested
- ✅ Queue depends on doctor arrival status

---

## 🌐 API Endpoints

### Basic CRUD Endpoints

#### GET /api/Appointments
**Description:** Get appointments with optional filtering  
**Authorization:** Assistant, Doctor (own), Admin  
**Query Parameters:**
- `doctorId` (int, optional)
- `patientId` (int, optional)
- `date` (DateOnly, optional)
- `status` (AppointmentStatus, optional)

**Response:** `List<AppointmentResponse>`

**Authorization Logic:**
- Patient role → Automatically filter by authenticated patient's ID
- Doctor role → Automatically filter by authenticated doctor's ID
- Assistant/Admin → Can view clinic appointments

---

#### GET /api/Appointments/{id}
**Description:** Get single appointment by ID  
**Authorization:** Patient (own), Assistant, Doctor, Admin  
**Response:** `AppointmentResponse`

**Authorization Logic:**
- Patient can only view their own appointments
- Service must check ownership

---

#### POST /api/Appointments
**Description:** Create/book new appointment  
**Authorization:** Patient (own), Assistant, Admin  
**Request:** `AppointmentRequest`  
**Response:** `201 Created` with `AppointmentResponse`

**Validations:** All creation validations (see Validation Rules)

---

#### PUT /api/Appointments/{id}
**Description:** Update existing appointment  
**Authorization:** Patient (own), Assistant, Admin  
**Request:** `AppointmentRequest`  
**Response:** `204 No Content`

**Business Rules:**
- Only `Scheduled` and `Confirmed` can be updated
- All booking validations re-run
- Conflict detection excludes current appointment

---

#### ~~DELETE /api/Appointments/{id}~~ ⚠️ **RECONSIDER**
**Recommendation:** Remove this endpoint entirely and use cancellation instead.

If kept:
- **Authorization:** Admin only
- **Allowed only for:** `Scheduled` or `Cancelled` appointments
- **Response:** `204 No Content`

**⚠️ Warning:**
- DELETE is NOT a normal business operation
- Use POST .../cancel instead
- Appointments are historical records

---

### Lifecycle Endpoints

#### POST /api/Appointments/{id}/check-in
**Description:** Check in patient (mark as arrived/waiting)  
**Authorization:** Assistant, Admin  
**Request:** None  
**Response:** `204 No Content`

**Business Logic:**
- Validates status is `Scheduled` or `Confirmed`
- Sets `CheckedInAt = DateTime.UtcNow`
- Changes status to `Waiting`

---

#### POST /api/Appointments/{id}/start
**Description:** Start appointment consultation  
**Authorization:** Doctor (own), Admin  
**Request:** None  
**Response:** `204 No Content`

**Business Logic:**
- Validates status is `Waiting`
- **Checks doctor has arrived** (via DoctorAttendance in Phase 6)
- Sets `ActualStartTime = DateTime.UtcNow`
- Changes status to `InProgress`

---

#### POST /api/Appointments/{id}/complete
**Description:** Complete appointment consultation  
**Authorization:** Doctor (own), Admin  
**Request:** None  
**Response:** `204 No Content`

**Business Logic:**
- Validates status is `InProgress`
- Sets `ActualEndTime = DateTime.UtcNow`
- Changes status to `Completed`

---

#### POST /api/Appointments/{id}/cancel
**Description:** Cancel appointment  
**Authorization:** Patient (own), Assistant, Doctor, Admin  
**Request:** None or `CancelRequest { Reason }` (optional)  
**Response:** `204 No Content`

**Business Logic:**
- Validates status is NOT `Completed`
- Changes status to `Cancelled`
- Optionally stores cancellation reason

---

### Queue Endpoints

#### GET /api/Appointments/doctor/{doctorId}/queue
**Description:** Get doctor's current queue  
**Authorization:** Assistant, Doctor (own), Admin  
**Query Parameters:**
- `date` (DateOnly, optional) - defaults to today

**Response:** `QueueResponse`

---

## 📄 Contracts/DTOs

### Files to Create
```
Contracts/
  └── Appointment/
      ├── AppointmentRequest.cs
      ├── AppointmentResponse.cs
      ├── CreateAppointmentRequestValidator.cs
      ├── UpdateAppointmentRequestValidator.cs
      ├── QueueResponse.cs
      └── QueueItemResponse.cs
```

### Contract Definitions

#### AppointmentRequest.cs
```csharp
namespace ClinicManagementSystem.api.Contracts.Appointment;

public record AppointmentRequest(
    int PatientId,
    int DoctorId,
    DateOnly AppointmentDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Notes
);
```

#### AppointmentResponse.cs
```csharp
namespace ClinicManagementSystem.api.Contracts.Appointment;

public record AppointmentResponse(
    int Id,
    int PatientId,
    string PatientName_En,
    string PatientName_Ar,
    int DoctorId,
    string DoctorName_En,
    string DoctorName_Ar,
    DateOnly AppointmentDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Status,
    DateTime? CheckedInAt,
    DateTime? ActualStartTime,
    DateTime? ActualEndTime,
    string? Notes,
    DateTime CreatedOn,
    DateTime? UpdatedOn
);
```

#### CreateAppointmentRequestValidator.cs
```csharp
using FluentValidation;

namespace ClinicManagementSystem.api.Contracts.Appointment;

public class CreateAppointmentRequestValidator : AbstractValidator<AppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("Valid Patient ID is required.");

        RuleFor(x => x.DoctorId)
            .GreaterThan(0)
            .WithMessage("Valid Doctor ID is required.");

        RuleFor(x => x.AppointmentDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Appointment date cannot be in the past.");

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage("Start time is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .WithMessage("End time is required.")
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes must not exceed 1000 characters.");
    }
}
```

#### QueueResponse.cs
```csharp
namespace ClinicManagementSystem.api.Contracts.Appointment;

public record QueueResponse(
    int DoctorId,
    string DoctorName_En,
    string DoctorName_Ar,
    QueueItemResponse? CurrentPatient,
    List<QueueItemResponse> WaitingPatients,
    int TotalWaiting,
    int EstimatedWaitingMinutes
);
```

#### QueueItemResponse.cs
```csharp
namespace ClinicManagementSystem.api.Contracts.Appointment;

public record QueueItemResponse(
    int AppointmentId,
    int PatientId,
    string PatientName_En,
    string PatientName_Ar,
    TimeOnly ScheduledTime,
    DateTime CheckedInAt,
    int PositionInQueue
);
```

---

## 🛠️ Abstractions Layer

### Files to Create
```
Abstractions/
  └── AppointmentErrors.cs
```

### AppointmentErrors.cs
```csharp
namespace ClinicManagementSystem.api.Abstractions;

public static class AppointmentErrors
{
    // ========== Basic CRUD Errors ==========
    public static readonly Error NotFound = new(
        "Appointment.NotFound",
        "Appointment not found",
        ErrorType.NotFound);

    // ========== Foreign Key Validation Errors ==========
    public static readonly Error PatientNotFound = new(
        "Appointment.PatientNotFound",
        "The specified patient does not exist",
        ErrorType.NotFound);

    public static readonly Error DoctorNotFound = new(
        "Appointment.DoctorNotFound",
        "The specified doctor does not exist",
        ErrorType.NotFound);

    // ========== Time Validation Errors ==========
    public static readonly Error InvalidTimeRange = new(
        "Appointment.InvalidTimeRange",
        "End time must be after start time",
        ErrorType.Validation);

    public static readonly Error PastAppointment = new(
        "Appointment.PastAppointment",
        "Cannot create appointment in the past",
        ErrorType.Validation);

    // ========== Conflict Errors ==========
    public static readonly Error DoctorConflict = new(
        "Appointment.DoctorConflict",
        "Doctor has another appointment at this time",
        ErrorType.Conflict);

    public static readonly Error PatientConflict = new(
        "Appointment.PatientConflict",
        "Patient has another appointment at this time",
        ErrorType.Conflict);

    // ========== Schedule Errors (Phase 5) ==========
    public static readonly Error DoctorNotScheduled = new(
        "Appointment.DoctorNotScheduled",
        "Doctor does not have a schedule for this day",
        ErrorType.Validation);

    public static readonly Error OutsideWorkingHours = new(
        "Appointment.OutsideWorkingHours",
        "Appointment is outside doctor's working hours",
        ErrorType.Validation);

    // ========== Check-In Errors ==========
    public static readonly Error CannotCheckIn = new(
        "Appointment.CannotCheckIn",
        "Can only check in scheduled or confirmed appointments",
        ErrorType.Validation);

    public static readonly Error AlreadyCheckedIn = new(
        "Appointment.AlreadyCheckedIn",
        "Patient has already checked in",
        ErrorType.Conflict);

    // ========== Start Appointment Errors ==========
    public static readonly Error CannotStart = new(
        "Appointment.CannotStart",
        "Can only start appointments that are in Waiting status",
        ErrorType.Validation);

    public static readonly Error AlreadyStarted = new(
        "Appointment.AlreadyStarted",
        "Appointment has already started",
        ErrorType.Conflict);

    public static readonly Error DoctorNotArrived = new(
        "Appointment.DoctorNotArrived",
        "Doctor has not arrived at the clinic yet",
        ErrorType.Validation);

    // ========== Complete Appointment Errors ==========
    public static readonly Error CannotComplete = new(
        "Appointment.CannotComplete",
        "Can only complete appointments that are in InProgress status",
        ErrorType.Validation);

    public static readonly Error AlreadyCompleted = new(
        "Appointment.AlreadyCompleted",
        "Appointment has already been completed",
        ErrorType.Conflict);

    // ========== Cancellation Errors ==========
    public static readonly Error CannotCancel = new(
        "Appointment.CannotCancel",
        "Cannot cancel completed appointments",
        ErrorType.Validation);

    public static readonly Error AlreadyCancelled = new(
        "Appointment.AlreadyCancelled",
        "Appointment has already been cancelled",
        ErrorType.Conflict);

    // ========== Update Errors ==========
    public static readonly Error CannotUpdate = new(
        "Appointment.CannotUpdate",
        "Cannot update appointments that are not in Scheduled or Confirmed status",
        ErrorType.Validation);

    // ========== Delete Errors ==========
    public static readonly Error CannotDelete = new(
        "Appointment.CannotDelete",
        "Cannot delete appointments that are not in Scheduled or Cancelled status",
        ErrorType.Validation);

    // ========== Authorization Errors ==========
    public static readonly Error Unauthorized = new(
        "Appointment.Unauthorized",
        "You are not authorized to access this appointment",
        ErrorType.Validation);
}
```

---

## 🔧 Service Layer

### Files to Create
```
Services/
  ├── IAppointmentService.cs
  └── AppointmentService.cs
```

### Service Responsibilities

The `AppointmentService` is responsible for:

1. **ALL business logic validation**
2. **Entity existence checks** (Patient, Doctor)
3. **Conflict detection** (Doctor overlap, Patient overlap)
4. **Schedule validation** (Phase 5)
5. **Status transition validation**
6. **Doctor arrival checking** (Phase 6)
7. **Queue calculation**
8. **Authorization enforcement** (ownership checks)

**The controller should NOT contain business logic.**

### IAppointmentService.cs (Interface)
```csharp
using ClinicManagementSystem.api.Abstractions;
using ClinicManagementSystem.api.Models;

namespace ClinicManagementSystem.api.Services;

public interface IAppointmentService
{
    // ========== Basic CRUD ==========
    Task<Result<IEnumerable<Appointment>>> GetAllAsync(
        int? doctorId, 
        int? patientId, 
        DateOnly? date, 
        AppointmentStatus? status,
        CancellationToken cancellationToken);
    
    Task<Result<Appointment>> GetAsync(int id, CancellationToken cancellationToken);
    
    Task<Result<Appointment>> CreateAsync(Appointment appointment, CancellationToken cancellationToken);
    
    Task<Result> UpdateAsync(int id, Appointment appointment, CancellationToken cancellationToken);
    
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);

    // ========== Lifecycle Actions ==========
    Task<Result> CheckInAsync(int id, CancellationToken cancellationToken);
    
    Task<Result> StartAppointmentAsync(int id, CancellationToken cancellationToken);
    
    Task<Result> CompleteAppointmentAsync(int id, CancellationToken cancellationToken);
    
    Task<Result> CancelAppointmentAsync(int id, CancellationToken cancellationToken);

    // ========== Queue Management ==========
    Task<Result<QueueInfo>> GetDoctorQueueAsync(int doctorId, DateOnly date, CancellationToken cancellationToken);
}

public record QueueInfo(
    Appointment? CurrentPatient,
    List<Appointment> WaitingPatients,
    int TotalWaiting,
    int EstimatedWaitingMinutes
);
```

### AppointmentService.cs Implementation Notes

**Phase Implementation:**
- **Phase 2:** Implement basic CRUD without doctor schedule validation
- **Phase 3:** Implement lifecycle actions (check-in, start, complete, cancel)
- **Phase 5:** Add doctor schedule validation to `CreateAsync` and `UpdateAsync`
- **Phase 6:** Add doctor arrival check to `StartAppointmentAsync`
- **Phase 7:** Implement queue management

**Key Methods:**

#### CreateAsync
```csharp
public async Task<Result<Appointment>> CreateAsync(Appointment appointment, CancellationToken cancellationToken)
{
    // 1. Validate Patient exists
    // 2. Validate Doctor exists
    // 3. Validate time range
    // 4. Validate not in past
    // 5. [Phase 5] Validate doctor schedule
    // 6. Check Doctor conflict
    // 7. Check Patient conflict
    // 8. Set Status = Scheduled
    // 9. Save
}
```

#### StartAppointmentAsync
```csharp
public async Task<Result> StartAppointmentAsync(int id, CancellationToken cancellationToken)
{
    // 1. Get appointment
    // 2. Validate status is Waiting
    // 3. [Phase 6] Check doctor has arrived (DoctorAttendance)
    // 4. Set ActualStartTime
    // 5. Set Status = InProgress
    // 6. Save
}
```

---

## 🎮 Controller Layer

### Files to Create
```
Controllers/
  └── AppointmentsController.cs
```

### Controller Responsibilities

The controller should be **thin** and only handle:
1. Routing
2. Authorization attributes
3. Request/Response mapping
4. HTTP status code selection
5. Calling service methods

**DO NOT** put business logic in the controller.

### AppointmentsController.cs Structure
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    private readonly IAppointmentService _appointmentService = appointmentService;

    [HttpGet("")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? doctorId,
        [FromQuery] int? patientId,
        [FromQuery] DateOnly? date,
        [FromQuery] AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        // Call service
        // Map to response
        // Return result
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        // Call service
        // Map to response
        // Return result
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(AppointmentRequest request, CancellationToken cancellationToken)
    {
        // Map request to entity
        // Call service
        // Return 201 Created
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AppointmentRequest request, CancellationToken cancellationToken)
    {
        // Map request to entity
        // Call service
        // Return 204 No Content
    }

    // DELETE - Reconsider keeping this endpoint

    [HttpPost("{id}/check-in")]
    public async Task<IActionResult> CheckIn(int id, CancellationToken cancellationToken)
    {
        // Call service
        // Return 204 No Content
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(int id, CancellationToken cancellationToken)
    {
        // Call service
        // Return 204 No Content
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        // Call service
        // Return 204 No Content
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        // Call service
        // Return 204 No Content
    }

    [HttpGet("doctor/{doctorId}/queue")]
    public async Task<IActionResult> GetDoctorQueue(
        int doctorId,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        // Call service
        // Map to QueueResponse
        // Return result
    }
}
```

---

## 🗄️ Entity Configuration

### Files to Create
```
EntitiesConfigurations/
  └── AppointmentConfiguration.cs
```

### AppointmentConfiguration.cs
```csharp
using ClinicManagementSystem.api.Models;
using ClinicManagementSystem.api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.api.EntitiesConfigurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.AppointmentDate)
            .IsRequired();

        builder.Property(a => a.StartTime)
            .IsRequired();

        builder.Property(a => a.EndTime)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(AppointmentStatus.Scheduled);

        builder.Property(a => a.CheckedInAt)
            .IsRequired(false);

        builder.Property(a => a.ActualStartTime)
            .IsRequired(false);

        builder.Property(a => a.ActualEndTime)
            .IsRequired(false);

        builder.Property(a => a.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);

        // ========== Relationships ==========
        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.StartTime })
            .HasDatabaseName("IX_Appointments_Doctor_Date_Time");

        builder.HasIndex(a => new { a.PatientId, a.AppointmentDate })
            .HasDatabaseName("IX_Appointments_Patient_Date");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_Appointments_Status");

        builder.HasIndex(a => new { a.Status, a.AppointmentDate })
            .HasDatabaseName("IX_Appointments_Status_Date");
    }
}
```

### Database Indexes Rationale
1. **Doctor + Date + Time:** For conflict detection and doctor's daily schedule
2. **Patient + Date:** For patient appointment history and conflict detection
3. **Status:** For filtering appointments by status
4. **Status + Date:** For queue queries and daily reports

**Important:** Indexes improve query performance but do NOT enforce business rules. Overlap prevention must be handled in the service layer.

---

## 🗂️ Database Updates

### ApplicationDbContext.cs
Add DbSet:
```csharp
public DbSet<Appointment> Appointments { get; set; } = null!;
```

Apply configuration:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
    // ... existing configurations
}
```

### Migration
```bash
dotnet ef migrations add AddAppointments
dotnet ef database update
```

---

## 🔌 Dependency Injection

### DependencyInjection.cs Updates
```csharp
// Services
services.AddScoped<IAppointmentService, AppointmentService>();

// Validators
services.AddScoped<IValidator<AppointmentRequest>, CreateAppointmentRequestValidator>();
```

---

## 🚀 Implementation Phases

### **Phase 1: Foundation Setup**
**Goal:** Set up the basic infrastructure

**Tasks:**
1. ✅ Create `AppointmentErrors.cs`
2. ✅ Create Contract classes (Request/Response/Validators)
3. ✅ Create `AppointmentConfiguration.cs`
4. ✅ Update `ApplicationDbContext` (add DbSet, apply configuration)
5. ✅ Create and run migration
6. ✅ Update DI registration
7. ✅ Verify database schema

**Duration:** 1-2 hours

---

### **Phase 2: Basic CRUD + Booking**
**Goal:** Implement appointment creation and basic operations

**Tasks:**
1. ✅ Create `IAppointmentService` interface
2. ✅ Implement `AppointmentService`:
   - `GetAllAsync` (with filtering)
   - `GetAsync`
   - `CreateAsync` (with all validations EXCEPT schedule)
   - `UpdateAsync` (with status checks)
   - `DeleteAsync` (optional, with restrictions)
3. ✅ Create `AppointmentsController`:
   - GET /api/Appointments
   - GET /api/Appointments/{id}
   - POST /api/Appointments
   - PUT /api/Appointments/{id}
   - DELETE /api/Appointments/{id} (if kept)
4. ✅ Test CRUD operations
5. ✅ Test conflict detection (doctor and patient)

**Duration:** 3-4 hours

---

### **Phase 3: Appointment Lifecycle**
**Goal:** Implement lifecycle actions (check-in, start, complete, cancel)

**Tasks:**
1. ✅ Implement in `AppointmentService`:
   - `CheckInAsync`
   - `StartAppointmentAsync` (WITHOUT doctor arrival check for now)
   - `CompleteAppointmentAsync`
   - `CancelAppointmentAsync`
2. ✅ Add lifecycle endpoints to controller:
   - POST /api/Appointments/{id}/check-in
   - POST /api/Appointments/{id}/start
   - POST /api/Appointments/{id}/complete
   - POST /api/Appointments/{id}/cancel
3. ✅ Test status transitions
4. ✅ Test invalid transitions are blocked

**Duration:** 2-3 hours

---

### **Phase 4: Authorization**
**Goal:** Implement role-based authorization

**Tasks:**
1. ✅ Add role claims to JWT tokens (if not already done)
2. ✅ Implement authorization logic in service:
   - Ownership checks for patients
   - Doctor filtering
   - Assistant clinic filtering (future)
3. ✅ Add `[Authorize(Roles = "...")]` attributes to controller actions
4. ✅ Test authorization:
   - Patient can only view/manage own appointments
   - Doctor can only start/complete own appointments
   - Assistant can manage clinic appointments
   - Admin has full access

**Duration:** 2-3 hours

---

### **Phase 5: Doctor Schedule Integration**
**Goal:** Add doctor schedule validation to appointment booking

**Tasks:**
1. ✅ Create `DoctorSchedule` model
2. ✅ Create `DoctorScheduleConfiguration`
3. ✅ Create migration
4. ✅ Create `IDoctorScheduleService` and `DoctorScheduleService`
5. ✅ Create `DoctorSchedulesController`
6. ✅ Update `AppointmentService.CreateAsync`:
   - Add schedule validation
   - Check appointment falls within working hours
7. ✅ Update `AppointmentService.UpdateAsync`:
   - Re-validate schedule
8. ✅ Add new errors:
   - `DoctorNotScheduled`
   - `OutsideWorkingHours`
9. ✅ Test schedule validation

**Duration:** 3-4 hours

---

### **Phase 6: Doctor Attendance Integration**
**Goal:** Add doctor arrival tracking and validation

**Tasks:**
1. ✅ Create `DoctorAttendance` model
2. ✅ Create `DoctorAttendanceConfiguration`
3. ✅ Create migration
4. ✅ Create `IDoctorAttendanceService` and `DoctorAttendanceService`
5. ✅ Create `DoctorAttendanceController` with endpoints:
   - POST /api/DoctorAttendance/check-in
   - POST /api/DoctorAttendance/check-out
   - GET /api/DoctorAttendance/today
6. ✅ Update `AppointmentService.StartAppointmentAsync`:
   - Add doctor arrival check
   - Return error if doctor not arrived
7. ✅ Add new error:
   - `DoctorNotArrived`
8. ✅ Test doctor arrival logic

**Duration:** 3-4 hours

---

### **Phase 7: Queue Management**
**Goal:** Implement queue view and estimated wait time

**Tasks:**
1. ✅ Implement `GetDoctorQueueAsync` in service:
   - Get current patient (InProgress)
   - Get waiting patients (Waiting, ordered by CheckedInAt)
   - Calculate positions
   - Calculate estimated wait time
2. ✅ Add queue endpoint to controller:
   - GET /api/Appointments/doctor/{doctorId}/queue
3. ✅ Create queue response mapping
4. ✅ Test queue:
   - With current patient
   - Without current patient
   - With multiple waiting patients
   - Verify position calculation
   - Verify wait time estimation

**Duration:** 2-3 hours

---

### **Phase 8: Testing & Validation**
**Goal:** Comprehensive testing of all business rules

**Test Categories:**
1. ✅ **Creation Validations:**
   - Patient exists
   - Doctor exists
   - Not in past
   - EndTime > StartTime
   - Doctor schedule (Phase 5)
   - Doctor conflict
   - Patient conflict

2. ✅ **Update Validations:**
   - Status allows update
   - All creation validations
   - Conflict excludes current appointment

3. ✅ **Lifecycle Transitions:**
   - Valid transitions
   - Invalid transitions blocked
   - Doctor arrival requirement (Phase 6)

4. ✅ **Authorization:**
   - Patient ownership
   - Doctor ownership
   - Role-based access

5. ✅ **Queue:**
   - Position calculation
   - Wait time estimation
   - Current patient handling

**Duration:** 2-3 hours

---

### **Phase 9: Performance & Refinement**
**Goal:** Optimize queries and refine implementation

**Tasks:**
1. ✅ Review and optimize database queries
2. ✅ Add missing indexes if needed
3. ✅ Review error messages
4. ✅ Add logging (optional)
5. ✅ Code cleanup
6. ✅ Documentation updates

**Duration:** 1-2 hours

---

## ✅ Testing Checklist

### Creation Tests
- [ ] Create appointment with valid data → Success
- [ ] Create appointment with non-existent patient → PatientNotFound
- [ ] Create appointment with non-existent doctor → DoctorNotFound
- [ ] Create appointment in past → PastAppointment
- [ ] Create appointment with EndTime < StartTime → InvalidTimeRange
- [ ] Create appointment outside doctor schedule → OutsideWorkingHours (Phase 5)
- [ ] Create appointment with doctor conflict → DoctorConflict
- [ ] Create appointment with patient conflict → PatientConflict
- [ ] Create appointment with valid overlapping different patients → Success

### Update Tests
- [ ] Update Scheduled appointment → Success
- [ ] Update Confirmed appointment → Success
- [ ] Update Waiting appointment → CannotUpdate
- [ ] Update InProgress appointment → CannotUpdate
- [ ] Update Completed appointment → CannotUpdate
- [ ] Update with doctor conflict (excluding self) → DoctorConflict
- [ ] Update with patient conflict (excluding self) → PatientConflict

### Lifecycle Tests
- [ ] Check-in Scheduled appointment → Success
- [ ] Check-in Confirmed appointment → Success
- [ ] Check-in Waiting appointment → AlreadyCheckedIn
- [ ] Check-in InProgress appointment → CannotCheckIn
- [ ] Start Waiting appointment (doctor arrived) → Success (Phase 6)
- [ ] Start Waiting appointment (doctor NOT arrived) → DoctorNotArrived (Phase 6)
- [ ] Start Scheduled appointment → CannotStart
- [ ] Start InProgress appointment → AlreadyStarted
- [ ] Complete InProgress appointment → Success
- [ ] Complete Waiting appointment → CannotComplete
- [ ] Complete Completed appointment → AlreadyCompleted
- [ ] Cancel Scheduled appointment → Success
- [ ] Cancel Waiting appointment → Success
- [ ] Cancel Completed appointment → CannotCancel

### Authorization Tests
- [ ] Patient GET all appointments → Only own appointments returned
- [ ] Patient GET other patient's appointment → Unauthorized
- [ ] Patient POST appointment for self → Success
- [ ] Patient POST appointment for others → Unauthorized (service validation)
- [ ] Doctor GET all appointments → Only own appointments returned
- [ ] Doctor start own appointment → Success
- [ ] Doctor start other doctor's appointment → Unauthorized
- [ ] Assistant GET appointments → Clinic appointments returned
- [ ] Assistant check-in appointment → Success
- [ ] Admin full access → Success

### Queue Tests
- [ ] Get queue with no appointments → Empty queue
- [ ] Get queue with InProgress → CurrentPatient populated
- [ ] Get queue with multiple Waiting → Correct order and positions
- [ ] Get queue estimated wait time → Calculated correctly

---

## 🎯 Future Features (Out of Scope for MVP)

These features are intentionally excluded from the initial implementation:

### 🚫 NOT Included in Current Plan
1. **Recurring Appointments**
   - Weekly/monthly recurring bookings
   - Requires separate RecurringAppointment model

2. **Appointment Templates**
   - Pre-defined appointment types with durations
   - Requires AppointmentType model

3. **Online Patient Booking**
   - Public-facing booking API
   - Requires authentication for patients
   - Requires payment integration

4. **Notifications**
   - SMS reminders
   - Email notifications
   - Push notifications
   - Requires notification service

5. **Appointment Confirmation**
   - Automatic status change Scheduled → Confirmed
   - Requires notification + patient response

6. **Advanced Queue Management**
   - Queue reordering
   - Priority patients
   - Emergency insertions

7. **Historical Analytics**
   - Average wait time per doctor
   - No-show rate tracking
   - Doctor performance metrics

8. **Multi-Clinic Support**
   - Assistant filtered by clinic
   - Requires clinic-based authorization

9. **Appointment Notes/Attachments**
   - Clinical notes during appointment
   - File attachments
   - Requires document storage

10. **Patient Self-Service**
    - Patient can reschedule
    - Patient can cancel with reason
    - Requires patient portal

---

## 📝 Summary

### What This Plan Includes:
✅ Complete appointment CRUD with business validations  
✅ Conflict detection (doctor and patient)  
✅ Lifecycle management (check-in, start, complete, cancel)  
✅ Role-based authorization  
✅ Doctor schedule validation (Phase 5)  
✅ Doctor attendance/arrival tracking (Phase 6)  
✅ Dynamic queue management  
✅ Estimated wait time calculation  

### Dependencies:
🔗 **DoctorSchedule** (Phase 5) - Required for schedule validation  
🔗 **DoctorAttendance** (Phase 6) - Required for start appointment  

### Total Estimated Time:
**20-28 hours** across 9 phases

### Key Principles:
1. **Business rules in service layer**
2. **Thin controllers**
3. **Result pattern for error handling**
4. **Authorization from the beginning**
5. **No over-engineering**
6. **Maintainable and testable code**

---

## ⚠️ Important Reminders

1. **DO NOT add unnecessary fields** to Appointment model
2. **DO NOT store calculated values** (QueuePosition, EstimatedWaitingTime)
3. **DO NOT use DELETE** as a normal operation (use cancellation)
4. **DO NOT allow patients to view all appointments**
5. **DO NOT hard-code** consultation duration (make it configurable)
6. **DO enforce ownership** at the service layer
7. **DO validate status transitions** strictly
8. **DO test authorization** thoroughly
9. **DO follow existing architecture patterns**
10. **DO NOT start implementing before plan approval**

---

**This plan is ready for review and approval before implementation begins.**
