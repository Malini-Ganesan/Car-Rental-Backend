using CarRentalAPI.Models;
using CarRentalAPI.Repositories.Interface;
using CarRentalAPI.Services;
using FluentAssertions;
using Moq;

namespace CarRentalAPI.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<ICarRepository> _mockCarRepo;
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _mockCarRepo = new Mock<ICarRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _service = new DashboardService(_mockCarRepo.Object, _mockBookingRepo.Object);
    }

    [Fact]
    public async Task GetDashboardData_ShouldReturnCorrectTotals()
    {
        // Arrange
        var cars = new List<Car>
        {
            new Car { Id = 1, Name = "Toyota Fortuner" },
            new Car { Id = 2, Name = "Honda Civic" }
        };

        var bookings = new List<Booking>
        {
            new Booking { Id = 1, CarId = 1, Location = "Chennai", TotalPrice = 50 },
            new Booking { Id = 2, CarId = 1, Location = "Mumbai",  TotalPrice = 80 },
            new Booking { Id = 3, CarId = 2, Location = "Delhi",   TotalPrice = 60 }
        };

        _mockCarRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(cars);
        _mockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _service.GetDashboardDataAsync();

        // Assert — use dynamic to access anonymous type
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        var doc = System.Text.Json.JsonDocument.Parse(json);

        doc.RootElement.GetProperty("totalCars").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("totalBookings").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task GetDashboardData_ShouldReturnZero_WhenNoCarsOrBookings()
    {
        _mockCarRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Car>());
        _mockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking>());

        var result = await _service.GetDashboardDataAsync();

        var json = System.Text.Json.JsonSerializer.Serialize(result);
        var doc = System.Text.Json.JsonDocument.Parse(json);

        doc.RootElement.GetProperty("totalCars").GetInt32().Should().Be(0);
        doc.RootElement.GetProperty("totalBookings").GetInt32().Should().Be(0);
    }
}