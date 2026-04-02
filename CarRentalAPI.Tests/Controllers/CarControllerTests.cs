using CarRentalAPI.Controllers;
using CarRentalAPI.DTOs;
using CarRentalAPI.Services;
using CarRentalAPI.Services.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarRentalAPI.Tests.Controllers;

public class CarControllerTests
{
    private readonly Mock<ICarService>    _mockCarService;
    private readonly Mock<NodeRedService> _mockNodeRed;
    private readonly CarController        _controller;

    public CarControllerTests()
    {
        _mockCarService = new Mock<ICarService>();
        _mockNodeRed    = new Mock<NodeRedService>(new HttpClient());
        _controller     = new CarController(_mockCarService.Object, _mockNodeRed.Object);
    }

    // ????? GET /api/car ?????

    [Fact]
    public void GetAll_ShouldReturn200_WithCarList()
    {
        var cars = new List<CarDto>
        {
            new CarDto { Id = 1, Name = "Toyota Fortuner", PricePerDay = 50 },
            new CarDto { Id = 2, Name = "Honda Civic",     PricePerDay = 40 }
        };

        _mockCarService.Setup(s => s.GetAll()).Returns(cars);

        var result = _controller.GetAll();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = ok.Value.Should().BeAssignableTo<IEnumerable<CarDto>>().Subject;
        returned.Should().HaveCount(2);
    }

    // ????? GET /api/car/{id} ?????

    [Fact]
    public void GetById_ShouldReturn200_WhenCarExists()
    {
        var car = new CarDto { Id = 1, Name = "Toyota Fortuner", PricePerDay = 50 };

        _mockCarService.Setup(s => s.GetById(1)).Returns(car);

        var result = _controller.GetById(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetById_ShouldReturn404_WhenCarNotFound()
    {
        _mockCarService.Setup(s => s.GetById(999)).Returns((CarDto)null!);

        var result = _controller.GetById(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ????? DELETE /api/car/{id} ?????

    [Fact]
    public void Delete_ShouldReturn204_WhenSuccessful()
    {
        _mockCarService.Setup(s => s.Delete(1));

        var result = _controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Delete_ShouldNotThrow_WhenCarExists()
    {
        _mockCarService.Setup(s => s.Delete(1));

        Action act = () => _controller.Delete(1);

        act.Should().NotThrow();
    }
}