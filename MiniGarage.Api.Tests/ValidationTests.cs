using System.ComponentModel.DataAnnotations;
using MiniGarage.Api.DTOs;
using MiniGarage.Api.Models;
using Xunit;

namespace MiniGarage.Api.Tests;

public class ValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }

    [Fact]
    public void CreateCarDto_WithValidData_PassesValidation()
    {
        // Arrange
        var dto = new CreateCarDto
        {
            OwnerName = "Khaled Nasser",
            CarModel = "Ford Mustang",
            LicensePlate = "KSA-9999",
            ProblemDescription = "Clutch slipping during acceleration",
            RepairStatus = RepairStatus.Pending
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("", "Model", "PLATE-1", "Problem description here", "Owner name is required.")]
    [InlineData("A", "Model", "PLATE-1", "Problem description here", "Owner name must be between 2 and 100 characters.")]
    [InlineData("Valid Owner", "", "PLATE-1", "Problem description here", "Car model is required.")]
    [InlineData("Valid Owner", "Model", "", "Problem description here", "License plate is required.")]
    [InlineData("Valid Owner", "Model", "PLATE-1", "", "Problem description is required.")]
    [InlineData("Valid Owner", "Model", "PLATE-1", "Bad", "Problem description must be between 5 and 500 characters.")]
    public void CreateCarDto_WithInvalidFields_FailsValidation(
        string owner, string model, string plate, string problem, string expectedErrorMessage)
    {
        // Arrange
        var dto = new CreateCarDto
        {
            OwnerName = owner,
            CarModel = model,
            LicensePlate = plate,
            ProblemDescription = problem,
            RepairStatus = RepairStatus.Pending
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.ErrorMessage == expectedErrorMessage);
    }

    [Fact]
    public void CreateCarDto_WithInvalidEnum_FailsValidation()
    {
        // Arrange
        var dto = new CreateCarDto
        {
            OwnerName = "Valid Owner",
            CarModel = "Valid Model",
            LicensePlate = "PLATE-99",
            ProblemDescription = "Valid problem description",
            RepairStatus = (RepairStatus)999 // Invalid enum value
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("RepairStatus must be 0"));
    }

    [Fact]
    public void UpdateCarDto_WithValidData_PassesValidation()
    {
        // Arrange
        var dto = new UpdateCarDto
        {
            OwnerName = "Fatima Al-Zahra",
            CarModel = "BMW 320i",
            LicensePlate = "B-4321",
            ProblemDescription = "Oil change and filter replacement",
            RepairStatus = RepairStatus.Completed
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        Assert.Empty(results);
    }
}
