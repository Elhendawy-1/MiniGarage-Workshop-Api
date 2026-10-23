using Microsoft.EntityFrameworkCore;
using MiniGarage.Api.Data;
using MiniGarage.Api.DTOs;
using MiniGarage.Api.Models;

namespace MiniGarage.Api.Domain;

/// <summary>
/// Deep implementation of IWorkshopRegistry.
/// Encapsulates Entity Framework Core persistence, change tracking optimization,
/// plate formatting invariants, and administrative key verification.
/// </summary>
public class WorkshopRegistry : IWorkshopRegistry
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WorkshopRegistry> _logger;

    public WorkshopRegistry(
        AppDbContext db,
        IConfiguration configuration,
        ILogger<WorkshopRegistry> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CarResponseDto>> ListVehiclesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WorkshopRegistry: Retrieving all vehicles from database.");

        return await _db.Cars
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
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CarResponseDto?> GetVehicleAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WorkshopRegistry: Fetching vehicle {VehicleId}.", id);

        return await _db.Cars
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
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CarResponseDto> RegisterVehicleAsync(CreateCarDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WorkshopRegistry: Registering new vehicle for {OwnerName}.", dto.OwnerName);

        var car = new Car
        {
            OwnerName = dto.OwnerName.Trim(),
            CarModel = dto.CarModel.Trim(),
            LicensePlate = CanonicalizePlate(dto.LicensePlate),
            ProblemDescription = dto.ProblemDescription.Trim(),
            RepairStatus = dto.RepairStatus,
            CreatedAt = DateTime.UtcNow
        };

        _db.Cars.Add(car);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("WorkshopRegistry: Vehicle persisted with generated ID {VehicleId}.", car.Id);

        return MapToResponse(car);
    }

    /// <inheritdoc />
    public async Task<CarResponseDto?> UpdateVehicleAsync(int id, UpdateCarDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WorkshopRegistry: Updating vehicle {VehicleId}.", id);

        var car = await _db.Cars
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (car is null)
        {
            _logger.LogWarning("WorkshopRegistry: Update failed - vehicle {VehicleId} not found.", id);
            return null;
        }

        // Apply canonicalization & sanitize fields (Protection against mass assignment)
        car.OwnerName = dto.OwnerName.Trim();
        car.CarModel = dto.CarModel.Trim();
        car.LicensePlate = CanonicalizePlate(dto.LicensePlate);
        car.ProblemDescription = dto.ProblemDescription.Trim();
        car.RepairStatus = dto.RepairStatus;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("WorkshopRegistry: Vehicle {VehicleId} updated successfully.", id);

        return MapToResponse(car);
    }

    /// <inheritdoc />
    public async Task<RetirementResult> RetireVehicleAsync(int id, string? adminKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WorkshopRegistry: Administrative retirement requested for vehicle {VehicleId}.", id);

        var expectedKey = _configuration["Authentication:AdminApiKey"] ?? "garage-admin-secret-2026";
        if (string.IsNullOrEmpty(adminKey) || adminKey != expectedKey)
        {
            _logger.LogWarning("WorkshopRegistry: Unauthorized retirement attempt on vehicle {VehicleId}.", id);
            return RetirementResult.Unauthorized;
        }

        var car = await _db.Cars.FindAsync([id], cancellationToken);
        if (car is null)
        {
            _logger.LogWarning("WorkshopRegistry: Retirement failed - vehicle {VehicleId} not found.", id);
            return RetirementResult.NotFound;
        }

        _db.Cars.Remove(car);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("WorkshopRegistry: Vehicle {VehicleId} retired successfully.", id);

        return RetirementResult.Success;
    }

    private static string CanonicalizePlate(string plate) =>
        plate.Trim().ToUpperInvariant();

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
