using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MiniGarage.Api.Domain;
using MiniGarage.Api.DTOs;

namespace MiniGarage.Api.Controllers;

/// <summary>
/// CarsController is a shallow HTTP Transport Adapter at the network seam (/api/cars).
/// Delegates all domain invariants, plate canonicalization, and SQL persistence
/// to the deep IWorkshopRegistry module.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class CarsController : ControllerBase
{
    private readonly IWorkshopRegistry _registry;
    private readonly ILogger<CarsController> _logger;

    public CarsController(IWorkshopRegistry registry, ILogger<CarsController> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    /// <summary>Get all registered workshop vehicles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CarResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CarResponseDto>>> GetAllCars(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET /api/cars: Forwarding to WorkshopRegistry.");
        var cars = await _registry.ListVehiclesAsync(cancellationToken);
        return Ok(cars);
    }

    /// <summary>Get a single vehicle by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CarResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CarResponseDto>> GetCarById(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET /api/cars/{CarId}: Forwarding to WorkshopRegistry.", id);
        var car = await _registry.GetVehicleAsync(id, cancellationToken);

        if (car is null)
        {
            return NotFound(new { message = $"Car with ID {id} was not found." });
        }

        return Ok(car);
    }

    /// <summary>Register a new car at the workshop.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CarResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CarResponseDto>> CreateCar([FromBody] CreateCarDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP POST /api/cars: Forwarding intake for {OwnerName}.", dto.OwnerName);
        var created = await _registry.RegisterVehicleAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(GetCarById),
            new { id = created.Id },
            created);
    }

    /// <summary>Update all fields of an existing vehicle.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CarResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CarResponseDto>> UpdateCar(int id, [FromBody] UpdateCarDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP PUT /api/cars/{CarId}: Forwarding update.", id);
        var updated = await _registry.UpdateVehicleAsync(id, dto, cancellationToken);

        if (updated is null)
        {
            return NotFound(new { message = $"Car with ID {id} was not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Delete a car by ID.
    /// Protected endpoint: Requires valid X-Admin-Key header.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCar(
        int id,
        [FromHeader(Name = "X-Admin-Key")] string? adminKey,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP DELETE /api/cars/{CarId}: Forwarding retirement request.", id);
        var result = await _registry.RetireVehicleAsync(id, adminKey, cancellationToken);

        return result switch
        {
            RetirementResult.Success => NoContent(),
            RetirementResult.Unauthorized => Unauthorized(new { message = "Unauthorized: A valid X-Admin-Key header is required to delete workshop records." }),
            RetirementResult.NotFound => NotFound(new { message = $"Car with ID {id} was not found." }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
