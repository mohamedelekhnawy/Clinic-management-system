using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.api.Examples;

/// <summary>
/// Example controller demonstrating proper logging usage with Serilog
/// This file is for reference only - copy these patterns to your actual controllers
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LoggingExampleController : ControllerBase
{
    private readonly ILogger<LoggingExampleController> _logger;

    public LoggingExampleController(ILogger<LoggingExampleController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Example: Basic information logging
    /// </summary>
    [HttpGet("basic")]
    public IActionResult BasicLogging()
    {
        _logger.LogInformation("Processing basic logging example");
        return Ok("Check the logs!");
    }

    /// <summary>
    /// Example: Structured logging with parameters
    /// </summary>
    [HttpGet("structured/{id}")]
    public IActionResult StructuredLogging(int id, [FromQuery] string? name)
    {
        // Good - Structured logging with named parameters
        _logger.LogInformation("Fetching resource with ID {ResourceId} and Name {ResourceName}", id, name);
        
        // Bad - Don't use string interpolation
        // _logger.LogInformation($"Fetching resource with ID {id}");
        
        return Ok(new { id, name });
    }

    /// <summary>
    /// Example: Different log levels
    /// </summary>
    [HttpGet("levels")]
    public IActionResult DifferentLevels()
    {
        _logger.LogDebug("This is a debug message - only in Development");
        _logger.LogInformation("This is an information message");
        _logger.LogWarning("This is a warning message");
        
        return Ok("Logged at different levels");
    }

    /// <summary>
    /// Example: Exception logging
    /// </summary>
    [HttpGet("error")]
    public IActionResult ErrorLogging()
    {
        try
        {
            _logger.LogInformation("Attempting operation that will fail");
            
            // Simulate an error
            throw new InvalidOperationException("This is a simulated error");
        }
        catch (Exception ex)
        {
            // Log the exception with context
            _logger.LogError(ex, "Failed to process request for resource {ResourceType}", "Example");
            
            return StatusCode(500, "An error occurred. Check the logs for details.");
        }
    }

    /// <summary>
    /// Example: Using log scopes for related operations
    /// </summary>
    [HttpPost("scope")]
    public IActionResult LoggingWithScope([FromBody] CreateResourceRequest request)
    {
        var operationId = Guid.NewGuid();
        
        // All logs within this scope will include the OperationId
        using (_logger.BeginScope("Operation {OperationId}", operationId))
        {
            _logger.LogInformation("Starting resource creation");
            
            try
            {
                _logger.LogDebug("Validating request data");
                
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    _logger.LogWarning("Resource name is missing or empty");
                    return BadRequest("Name is required");
                }
                
                _logger.LogInformation("Creating resource {ResourceName}", request.Name);
                
                // Simulate processing
                System.Threading.Thread.Sleep(100);
                
                _logger.LogInformation("Resource created successfully with ID {ResourceId}", 123);
                
                return Ok(new { id = 123, name = request.Name, operationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create resource");
                throw;
            }
        }
    }

    /// <summary>
    /// Example: Performance logging
    /// </summary>
    [HttpGet("performance")]
    public async Task<IActionResult> PerformanceLogging()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        _logger.LogInformation("Starting expensive operation");
        
        // Simulate some work
        await Task.Delay(500);
        
        stopwatch.Stop();
        
        if (stopwatch.ElapsedMilliseconds > 300)
        {
            _logger.LogWarning("Operation took longer than expected: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation("Operation completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }
        
        return Ok(new { elapsedMs = stopwatch.ElapsedMilliseconds });
    }

    /// <summary>
    /// Example: Logging business events
    /// </summary>
    [HttpPost("business-event")]
    public IActionResult BusinessEventLogging([FromBody] AppointmentRequest request)
    {
        _logger.LogInformation(
            "Appointment scheduled for Patient {PatientId} with Doctor {DoctorId} at {AppointmentDate}",
            request.PatientId,
            request.DoctorId,
            request.AppointmentDate
        );
        
        // In production, you might have different log messages for important business events
        if (request.IsUrgent)
        {
            _logger.LogWarning(
                "URGENT appointment scheduled for Patient {PatientId}",
                request.PatientId
            );
        }
        
        return Ok();
    }
}

// Example request models
public record CreateResourceRequest(string Name, string? Description);
public record AppointmentRequest(int PatientId, int DoctorId, DateTime AppointmentDate, bool IsUrgent);
