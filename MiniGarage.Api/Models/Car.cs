namespace MiniGarage.Api.Models;

/// <summary>
/// Represents a car registered at the Mini Garage workshop.
/// This is the EF Core entity — it maps directly to the "Cars" table in SQL Server.
/// </summary>
public class Car
{
    /// <summary>Primary key. EF Core uses convention: property named "Id" → IDENTITY column.</summary>
    public int Id { get; set; }

    /// <summary>Full name of the car owner.</summary>
    public string OwnerName { get; set; } = string.Empty;

    /// <summary>Car make and model, e.g. "Toyota Corolla".</summary>
    public string CarModel { get; set; } = string.Empty;

    /// <summary>Unique license plate number.</summary>
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>Description of the problem the car was brought in for.</summary>
    public string ProblemDescription { get; set; } = string.Empty;

    /// <summary>Current repair status of the car in the workshop.</summary>
    public RepairStatus RepairStatus { get; set; } = RepairStatus.Pending;

    /// <summary>Date and time the car was registered in the system (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents the possible repair statuses for a car in the workshop.
/// Stored in the database as an integer (0, 1, 2) by EF Core convention.
/// </summary>
public enum RepairStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2
}
