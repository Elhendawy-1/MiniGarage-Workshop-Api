using System.ComponentModel.DataAnnotations;
using MiniGarage.Api.Models;

namespace MiniGarage.Api.DTOs;

/// <summary>
/// DTO (Data Transfer Object) for creating a new car entry.
/// 
/// Why a DTO instead of the Car entity directly?
/// - We never expose internal entity IDs or auto-generated fields (like CreatedAt) to the caller.
/// - We control exactly which fields the client is allowed to supply.
/// - Validation annotations live here, keeping the domain model clean.
/// 
/// ASP.NET Core's model binder reads the JSON body and maps it to this object.
/// The [ApiController] attribute then automatically validates Data Annotations
/// and returns a 400 Bad Request if validation fails — before the action even runs.
/// </summary>
public class CreateCarDto
{
    [Required(ErrorMessage = "Owner name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Owner name must be between 2 and 100 characters.")]
    public string OwnerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Car model is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Car model must be between 2 and 100 characters.")]
    public string CarModel { get; set; } = string.Empty;

    [Required(ErrorMessage = "License plate is required.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "License plate must be between 2 and 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", ErrorMessage = "License plate contains invalid characters.")]
    public string LicensePlate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Problem description is required.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Problem description must be between 5 and 500 characters.")]
    public string ProblemDescription { get; set; } = string.Empty;

    /// <summary>
    /// Optional initial status. Defaults to Pending if omitted.
    /// EnumDataType ensures only valid RepairStatus values are accepted.
    /// </summary>
    [EnumDataType(typeof(RepairStatus), ErrorMessage = "RepairStatus must be 0 (Pending), 1 (InProgress), or 2 (Completed).")]
    public RepairStatus RepairStatus { get; set; } = RepairStatus.Pending;
}
