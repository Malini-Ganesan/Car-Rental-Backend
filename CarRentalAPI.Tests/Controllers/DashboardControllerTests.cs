using CarRentalAPI.Controllers;
using CarRentalAPI.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarRentalAPI.Tests.Controllers;

public class DashboardControllerTests
{
    private readonly Mock<IDashboardService> _mockDashboardService;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mockDashboardService = new Mock<IDashboardService>();
        _controller = new DashboardController(_mockDashboardService.Object);
    }

    // ????? GET /api/dashboard/summary ?????

    [Fact]
    public async Task GetDashboardSummary_ShouldReturn200_WithData()
    {
        // Arrange
        var fakeData = new
        {
            totalCars = 5,
            totalBookings = 12,
            bookingsPerCar = new[]
            {
                new { Name = "Toyota Fortuner", BookingCount = 7 },
                new { Name = "Honda Civic",     BookingCount = 5 }
            }
        };

        _mockDashboardService
            .Setup(s => s.GetDashboardDataAsync())
            .ReturnsAsync(fakeData);

        // Act
        var result = await _controller.GetDashboardSummary();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboardSummary_ShouldReturn200_WhenNoData()
    {
        // Arrange
        var emptyData = new
        {
            totalCars = 0,
            totalBookings = 0,
            bookingsPerCar = Array.Empty<object>()
        };

        _mockDashboardService
            .Setup(s => s.GetDashboardDataAsync())
            .ReturnsAsync(emptyData);

        // Act
        var result = await _controller.GetDashboardSummary();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;

        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        var doc = System.Text.Json.JsonDocument.Parse(json);

        doc.RootElement.GetProperty("totalCars").GetInt32().Should().Be(0);
        doc.RootElement.GetProperty("totalBookings").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardSummary_ShouldCallServiceExactlyOnce()
    {
        // Arrange
        _mockDashboardService
            .Setup(s => s.GetDashboardDataAsync())
            .ReturnsAsync(new { totalCars = 1, totalBookings = 1 });

        // Act
        await _controller.GetDashboardSummary();

        // Assert — verify service was called exactly once
        _mockDashboardService.Verify(s => s.GetDashboardDataAsync(), Times.Once);
    }
}