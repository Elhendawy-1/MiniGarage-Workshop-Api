using MiniGarage.Api.Models;

namespace MiniGarage.Api.DTOs;

/// <summary>
/// DTO returned to the client for every car response.
/// 
/// Why a response DTO?
/// - We control what the client sees. We never accidentally expose
///   EF Core navigation properties, internal fields, or future sensitive data.
/// - We can shape the response differently from the database schema
///   (e.g., formatting RepairStatus as a string label).
/// - Decouples the API contract from the database model.
/// </summary>
public class CarResponseDto
{
    public int Id { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;

    /// <summary>Numeric value stored in DB (0, 1, 2).</summary>
    public RepairStatus RepairStatus { get; set; }

    /// <summary>Human-readable label derived from the enum — no extra library needed.</summary>
    public string RepairStatusLabel => RepairStatus.ToString();

    public DateTime CreatedAt { get; set; }
}
