using System.ComponentModel.DataAnnotations;
using MiniGarage.Api.Models;

namespace MiniGarage.Api.DTOs;

/// <summary>
/// DTO for updating an existing car (PUT /api/cars/{id}).
/// 
/// All fields are required because this is a full replacement (PUT semantics).
/// If you wanted partial updates, you would use PATCH with a different DTO
/// where fields are nullable/optional.
/// </summary>
public class UpdateCarDto
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

    [Required(ErrorMessage = "RepairStatus is required.")]
    [EnumDataType(typeof(RepairStatus), ErrorMessage = "RepairStatus must be 0 (Pending), 1 (InProgress), or 2 (Completed).")]
    public RepairStatus RepairStatus { get; set; }
}
