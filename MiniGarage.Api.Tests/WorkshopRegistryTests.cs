using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MiniGarage.Api.Data;
using MiniGarage.Api.Domain;
using MiniGarage.Api.DTOs;
using MiniGarage.Api.Models;
using Xunit;

namespace MiniGarage.Api.Tests;

public class WorkshopRegistryTests
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly WorkshopRegistry _registry;

    public WorkshopRegistryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        var configValues = new Dictionary<string, string?>
        {
            ["Authentication:AdminApiKey"] = "test-workshop-admin-key"
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        _registry = new WorkshopRegistry(_db, _config, NullLogger<WorkshopRegistry>.Instance);
    }

    [Fact]
    public async Task RegisterVehicleAsync_CanonicalizesLicensePlate_AndStoresUtcTimestamp()
    {
        // Arrange
        var dto = new CreateCarDto
        {
            OwnerName = "  Khaled Salem  ",
            CarModel = " Lexus LX600 ",
            LicensePlate = "  dxb-9900  ",
            ProblemDescription = "Air suspension inspection required.",
            RepairStatus = RepairStatus.Pending
        };

        // Act
        var result = await _registry.RegisterVehicleAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Khaled Salem", result.OwnerName);
        Assert.Equal("Lexus LX600", result.CarModel);
        Assert.Equal("DXB-9900", result.LicensePlate); // Verified canonicalization
        Assert.Equal(RepairStatus.Pending, result.RepairStatus);
        Assert.True((DateTime.UtcNow - result.CreatedAt).TotalSeconds < 5);
    }

    [Fact]
    public async Task ListVehiclesAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        _db.Cars.AddRange(
            new Car
            {
                Id = 1,
                OwnerName = "Owner 1",
                CarModel = "Model 1",
                LicensePlate = "A-1",
                ProblemDescription = "Issue 1",
                RepairStatus = RepairStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            },
            new Car
            {
                Id = 2,
                OwnerName = "Owner 2",
                CarModel = "Model 2",
                LicensePlate = "B-2",
                ProblemDescription = "Issue 2",
                RepairStatus = RepairStatus.InProgress,
                CreatedAt = DateTime.UtcNow
            }
        );
        await _db.SaveChangesAsync();

        // Act
        var list = await _registry.ListVehiclesAsync();

        // Assert
        Assert.Equal(2, list.Count);
        Assert.Equal(2, list[0].Id); // Most recent first
        Assert.Equal(1, list[1].Id);
    }

    [Fact]
    public async Task RetireVehicleAsync_ReturnsUnauthorized_WhenKeyIsInvalid()
    {
        // Act
        var result = await _registry.RetireVehicleAsync(1, "wrong-key");

        // Assert
        Assert.Equal(RetirementResult.Unauthorized, result);
    }

    [Fact]
    public async Task RetireVehicleAsync_ReturnsNotFound_WhenCarDoesNotExist()
    {
        // Act
        var result = await _registry.RetireVehicleAsync(999, "test-workshop-admin-key");

        // Assert
        Assert.Equal(RetirementResult.NotFound, result);
    }

    [Fact]
    public async Task RetireVehicleAsync_RemovesCar_WhenKeyIsValid()
    {
        // Arrange
        var car = new Car
        {
            OwnerName = "Owner Deletion",
            CarModel = "Model X",
            LicensePlate = "DEL-1",
            ProblemDescription = "Scrap",
            RepairStatus = RepairStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };
        _db.Cars.Add(car);
        await _db.SaveChangesAsync();

        // Act
        var result = await _registry.RetireVehicleAsync(car.Id, "test-workshop-admin-key");

        // Assert
        Assert.Equal(RetirementResult.Success, result);
        var inDb = await _db.Cars.FindAsync(car.Id);
        Assert.Null(inDb);
    }
}
