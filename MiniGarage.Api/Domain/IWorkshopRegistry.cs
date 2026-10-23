using MiniGarage.Api.DTOs;

namespace MiniGarage.Api.Domain;

/// <summary>
/// Result outcomes for administrative vehicle retirement.
/// </summary>
public enum RetirementResult
{
    Success,
    Unauthorized,
    NotFound
}

/// <summary>
/// Deep module interface for workshop vehicle lifecycle management.
/// Hides plate canonicalization, invariant validation, change tracking, and SQL Server persistence.
/// </summary>
public interface IWorkshopRegistry
{
    /// <summary>List all registered vehicles ordered by creation timestamp descending.</summary>
    Task<IReadOnlyList<CarResponseDto>> ListVehiclesAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieve a single vehicle by identifier.</summary>
    Task<CarResponseDto?> GetVehicleAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Register a new vehicle with canonicalized plate and sanitized description.</summary>
    Task<CarResponseDto> RegisterVehicleAsync(CreateCarDto dto, CancellationToken cancellationToken = default);

    /// <summary>Update domain fields of an existing vehicle, preventing mass-assignment leaks.</summary>
    Task<CarResponseDto?> UpdateVehicleAsync(int id, UpdateCarDto dto, CancellationToken cancellationToken = default);

    /// <summary>Permanently retire a vehicle subject to administrative key verification.</summary>
    Task<RetirementResult> RetireVehicleAsync(int id, string? adminKey, CancellationToken cancellationToken = default);
}
