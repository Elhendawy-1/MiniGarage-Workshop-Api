using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniGarage.Api.Data;
using MiniGarage.Api.DTOs;
using MiniGarage.Api.Models;
using Xunit;

namespace MiniGarage.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(AppDbContext)).ToList();

            foreach (var d in descriptors)
            {
                services.Remove(d);
            }

            // Create an isolated internal service provider for InMemory EF Core
            var inMemoryServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
                options.UseInternalServiceProvider(inMemoryServiceProvider);
            });
        });
    }
}

public class CarsApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CarsApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();

        // Ensure database is created and seeded for this test run
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetAllCars_ReturnsOk_WithListOfCars()
    {
        // Act
        var response = await _client.GetAsync("/api/cars");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cars = await response.Content.ReadFromJsonAsync<List<CarResponseDto>>();
        Assert.NotNull(cars);
        Assert.NotEmpty(cars);
    }

    [Fact]
    public async Task GetCarById_ExistingId_ReturnsOk_WithCar()
    {
        // Act (ID 1 seeded in AppDbContext)
        var response = await _client.GetAsync("/api/cars/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var car = await response.Content.ReadFromJsonAsync<CarResponseDto>();
        Assert.NotNull(car);
        Assert.Equal(1, car.Id);
        Assert.Equal("Ahmed Al-Rashidi", car.OwnerName);
    }

    [Fact]
    public async Task GetCarById_NonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/cars/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCar_ValidData_ReturnsCreated_WithLocationHeader()
    {
        // Arrange
        var newCar = new CreateCarDto
        {
            OwnerName = "Mariam Mansour",
            CarModel = "Nissan Altima",
            LicensePlate = "MAR-4455",
            ProblemDescription = "AC compressor making squeaking noise",
            RepairStatus = RepairStatus.Pending
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cars", newCar);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var createdCar = await response.Content.ReadFromJsonAsync<CarResponseDto>();
        Assert.NotNull(createdCar);
        Assert.True(createdCar.Id > 0);
        Assert.Equal(newCar.OwnerName, createdCar.OwnerName);
        Assert.Equal(newCar.LicensePlate, createdCar.LicensePlate);
    }

    [Fact]
    public async Task CreateCar_InvalidData_ReturnsBadRequest()
    {
        // Arrange (missing required fields)
        var invalidCar = new CreateCarDto
        {
            OwnerName = "",
            CarModel = "",
            LicensePlate = "",
            ProblemDescription = "Too short"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cars", invalidCar);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCar_ValidData_ReturnsOk_WithUpdatedCar()
    {
        // Arrange
        var updateDto = new UpdateCarDto
        {
            OwnerName = "Ahmed Al-Rashidi (Updated)",
            CarModel = "Toyota Corolla 2024",
            LicensePlate = "ABC-1234",
            ProblemDescription = "Engine repair completed and oil topped up",
            RepairStatus = RepairStatus.Completed
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/cars/1", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CarResponseDto>();
        Assert.NotNull(updated);
        Assert.Equal(1, updated.Id);
        Assert.Equal("Toyota Corolla 2024", updated.CarModel);
        Assert.Equal(RepairStatus.Completed, updated.RepairStatus);
    }

    [Fact]
    public async Task UpdateCar_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateCarDto
        {
            OwnerName = "Nobody",
            CarModel = "Ghost Car",
            LicensePlate = "GHO-0000",
            ProblemDescription = "Non-existent vehicle",
            RepairStatus = RepairStatus.Pending
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/cars/9999", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCar_WithoutAdminKey_ReturnsUnauthorized()
    {
        // Act (Attempt deletion without X-Admin-Key header)
        var response = await _client.DeleteAsync("/api/cars/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCar_ExistingId_WithAdminKey_ReturnsNoContent()
    {
        // Arrange: Create a temporary car to delete
        var tempCar = new CreateCarDto
        {
            OwnerName = "To Delete",
            CarModel = "Test Model",
            LicensePlate = "DEL-9999",
            ProblemDescription = "Car to be deleted in test",
            RepairStatus = RepairStatus.Pending
        };
        var createResponse = await _client.PostAsJsonAsync("/api/cars", tempCar);
        var created = await createResponse.Content.ReadFromJsonAsync<CarResponseDto>();
        Assert.NotNull(created);

        // Act with valid admin key
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/cars/{created.Id}");
        request.Headers.Add("X-Admin-Key", "garage-admin-secret-2026");
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify car is gone
        var getResponse = await _client.GetAsync($"/api/cars/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCar_NonExistentId_WithAdminKey_ReturnsNotFound()
    {
        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/cars/9999");
        request.Headers.Add("X-Admin-Key", "garage-admin-secret-2026");
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
