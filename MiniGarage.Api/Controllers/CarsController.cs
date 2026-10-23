using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MiniGarage.Api.Data;
using MiniGarage.Api.DTOs;
using MiniGarage.Api.Models;

namespace MiniGarage.Api.Controllers;

/// <summary>
/// CarsController handles all HTTP requests for the /api/cars route.
/// Protected with rate limiting, input validation, and function-level authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class CarsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<CarsController> _logger;
    private readonly IConfiguration _configuration;

    public CarsController(AppDbContext db, ILogger<CarsController> logger, IConfiguration configuration)
    {
        _db = db;
        _logger = logger;
        _configuration = configuration;
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/cars
    // ─────────────────────────────────────────────────────────────
    /// <summary>Get all registered cars.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CarResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CarResponseDto>>> GetAllCars()
    {
        _logger.LogInformation("Fetching all cars from the database.");

        // Optimized Query: AsNoTracking() avoids tracker allocation.
        // Direct projection into CarResponseDto pushes column selection into SQL Server (SELECT only needed columns).
        var cars = await _db.Cars
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CarResponseDto
            {
                Id = c.Id,
                OwnerName = c.OwnerName,
                CarModel = c.CarModel,
                LicensePlate = c.LicensePlate,
                ProblemDescription = c.ProblemDescription,
                RepairStatus = c.RepairStatus,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(cars);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/cars/{id}
    // ─────────────────────────────────────────────────────────────
    /// <summary>Get a single car by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CarResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CarResponseDto>> GetCarById(int id)
    {
        _logger.LogInformation("Fetching car with ID {CarId}.", id);

        // Optimized Query: Direct server-side projection with AsNoTracking()
        var car = await _db.Cars
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CarResponseDto
            {
                Id = c.Id,
                OwnerName = c.OwnerName,
                CarModel = c.CarModel,
                LicensePlate = c.LicensePlate,
                ProblemDescription = c.ProblemDescription,
                RepairStatus = c.RepairStatus,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (car is null)
        {
            _logger.LogWarning("Car with ID {CarId} was not found.", id);
            return NotFound(new { message = $"Car with ID {id} was not found." });
        }

        return Ok(car);
    }

    // ─────────────────────────────────────────────────────────────
    // POST /api/cars
    // ─────────────────────────────────────────────────────────────
    /// <summary>Register a new car at the workshop.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CarResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CarResponseDto>> CreateCar([FromBody] CreateCarDto dto)
    {
        _logger.LogInformation("Creating a new car for owner: {OwnerName}.", dto.OwnerName);

        var car = new Car
        {
            OwnerName = dto.OwnerName.Trim(),
            CarModel = dto.CarModel.Trim(),
            LicensePlate = dto.LicensePlate.Trim().ToUpperInvariant(),
            ProblemDescription = dto.ProblemDescription.Trim(),
            RepairStatus = dto.RepairStatus,
            CreatedAt = DateTime.UtcNow
        };

        _db.Cars.Add(car);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Car created successfully with ID {CarId}.", car.Id);

        return CreatedAtAction(
            nameof(GetCarById),
            new { id = car.Id },
            MapToResponse(car));
    }

    // ─────────────────────────────────────────────────────────────
    // PUT /api/cars/{id}
    // ─────────────────────────────────────────────────────────────
    /// <summary>Update all fields of an existing car.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CarResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CarResponseDto>> UpdateCar(int id, [FromBody] UpdateCarDto dto)
    {
        _logger.LogInformation("Updating car with ID {CarId}.", id);

        // Explicit AsTracking() for modification
        var car = await _db.Cars
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car is null)
        {
            _logger.LogWarning("Update failed: Car with ID {CarId} was not found.", id);
            return NotFound(new { message = $"Car with ID {id} was not found." });
        }

        // Prevent Mass Assignment: explicitly update only authorized domain fields
        car.OwnerName = dto.OwnerName.Trim();
        car.CarModel = dto.CarModel.Trim();
        car.LicensePlate = dto.LicensePlate.Trim().ToUpperInvariant();
        car.ProblemDescription = dto.ProblemDescription.Trim();
        car.RepairStatus = dto.RepairStatus;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Car with ID {CarId} updated successfully.", id);

        return Ok(MapToResponse(car));
    }

    // ─────────────────────────────────────────────────────────────
    // DELETE /api/cars/{id} (Requires Admin Authorization Header)
    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Delete a car by ID.
    /// Protected endpoint: Requires valid X-Admin-Key header (OWASP API5:2023 mitigation).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCar(int id, [FromHeader(Name = "X-Admin-Key")] string? adminKey)
    {
        _logger.LogInformation("Attempting to delete car with ID {CarId}.", id);

        var expectedKey = _configuration["Authentication:AdminApiKey"] ?? "garage-admin-secret-2026";
        if (string.IsNullOrEmpty(adminKey) || adminKey != expectedKey)
        {
            _logger.LogWarning("Unauthorized delete attempt on car {CarId}. Missing or invalid X-Admin-Key.", id);
            return Unauthorized(new { message = "Unauthorized: A valid X-Admin-Key header is required to delete workshop records." });
        }

        var car = await _db.Cars.FindAsync(id);

        if (car is null)
        {
            _logger.LogWarning("Delete failed: Car with ID {CarId} was not found.", id);
            return NotFound(new { message = $"Car with ID {id} was not found." });
        }

        _db.Cars.Remove(car);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Car with ID {CarId} deleted successfully by admin.", id);

        return NoContent();
    }

    private static CarResponseDto MapToResponse(Car car) => new()
    {
        Id = car.Id,
        OwnerName = car.OwnerName,
        CarModel = car.CarModel,
        LicensePlate = car.LicensePlate,
        ProblemDescription = car.ProblemDescription,
        RepairStatus = car.RepairStatus,
        CreatedAt = car.CreatedAt
    };
}
