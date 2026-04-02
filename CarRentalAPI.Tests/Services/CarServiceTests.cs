using CarRentalAPI.DTOs;
using CarRentalAPI.Models;
using CarRentalAPI.Repositories.Interface;
using CarRentalAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace CarRentalAPI.Tests.Services;

public class CarServiceTests
{
    private readonly Mock<ICarRepository> _mockRepo;
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly CarService _service;

    public CarServiceTests()
    {
        _mockRepo = new Mock<ICarRepository>();
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        _service = new CarService(_mockRepo.Object, _mockEnv.Object);
    }

    // ????? GetAll ?????

    [Fact]
    public void GetAll_ShouldReturnAllCarDtos()
    {
        var cars = new List<Car>
        {
            new Car { Id = 1, Name = "Toyota Fortuner", PricePerHour = 10, PricePerDay = 50, PricePerWeek = 300,
                      Category = new CarCategory { Name = "SUV" },
                      InsurancePlan = new InsurancePlan { Name = "Basic", CostPerDay = 10 } },
            new Car { Id = 2, Name = "Honda Civic",     PricePerHour = 8,  PricePerDay = 40, PricePerWeek = 250,
                      Category = new CarCategory { Name = "Sedan" },
                      InsurancePlan = new InsurancePlan { Name = "Standard", CostPerDay = 15 } }
        };

        _mockRepo.Setup(r => r.GetAll()).Returns(cars);

        var result = _service.GetAll().ToList();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Toyota Fortuner");
        result[1].CategoryName.Should().Be("Sedan");
    }

    [Fact]
    public void GetAll_ShouldReturnEmpty_WhenNoCars()
    {
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Car>());

        var result = _service.GetAll();

        result.Should().BeEmpty();
    }

    // ????? GetById ?????

    [Fact]
    public void GetById_ShouldReturnCarDto_WhenExists()
    {
        var car = new Car
        {
            Id = 1,
            Name = "Toyota Fortuner",
            PricePerHour = 10,
            PricePerDay = 50,
            PricePerWeek = 300,
            Category = new CarCategory { Name = "SUV" },
            InsurancePlan = new InsurancePlan { Name = "Basic", CostPerDay = 10 }
        };

        _mockRepo.Setup(r => r.GetById(1)).Returns(car);

        var result = _service.GetById(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Toyota Fortuner");
        result.PricePerDay.Should().Be(50);
        result.InsuranceName.Should().Be("Basic");
    }

    [Fact]
    public void GetById_ShouldReturnNull_WhenNotFound()
    {
        _mockRepo.Setup(r => r.GetById(999)).Returns((Car)null!);

        var result = _service.GetById(999);

        result.Should().BeNull();
    }

    // ????? Create ?????

    [Fact]
    public void Create_ShouldReturnCarDto_WhenNoImage()
    {
        var dto = new CarCreateDto
        {
            Name = "BMW X5",
            CategoryId = 1,
            InsurancePlanId = 1,
            PricePerHour = 20,
            PricePerDay = 100,
            PricePerWeek = 600,
            Image = null  // no image upload
        };

        _mockRepo.Setup(r => r.Add(It.IsAny<Car>()));
        _mockRepo.Setup(r => r.Save());

        // GetById called after save to return DTO
        _mockRepo.Setup(r => r.GetById(0)).Returns(new Car
        {
            Id = 0,
            Name = "BMW X5",
            CategoryId = 1,
            InsurancePlanId = 1,
            PricePerHour = 20,
            PricePerDay = 100,
            PricePerWeek = 600,
            Category = new CarCategory { Name = "SUV" },
            InsurancePlan = new InsurancePlan { Name = "Basic", CostPerDay = 10 }
        });

        var result = _service.Create(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("BMW X5");
        _mockRepo.Verify(r => r.Add(It.IsAny<Car>()), Times.Once);
        _mockRepo.Verify(r => r.Save(), Times.Once);
    }

    // ????? Update ?????

    [Fact]
    public void Update_ShouldModifyCar_WhenExists()
    {
        var existing = new Car { Id = 1, Name = "Old Name", CategoryId = 1, InsurancePlanId = 1 };

        _mockRepo.Setup(r => r.GetById(1)).Returns(existing);
        _mockRepo.Setup(r => r.Update(existing));
        _mockRepo.Setup(r => r.Save());

        var dto = new CarCreateDto
        {
            Name = "New Name",
            CategoryId = 2,
            InsurancePlanId = 1,
            PricePerHour = 15,
            PricePerDay = 70,
            PricePerWeek = 400,
            Image = null
        };

        _service.Update(1, dto);

        existing.Name.Should().Be("New Name");
        existing.CategoryId.Should().Be(2);
        _mockRepo.Verify(r => r.Update(existing), Times.Once);
        _mockRepo.Verify(r => r.Save(), Times.Once);
    }

    [Fact]
    public void Update_ShouldThrow_WhenCarNotFound()
    {
        _mockRepo.Setup(r => r.GetById(999)).Returns((Car)null!);

        var dto = new CarCreateDto { Name = "Test", CategoryId = 1, InsurancePlanId = 1 };

        Action act = () => _service.Update(999, dto);

        act.Should().Throw<Exception>().WithMessage("Car not found");
    }

    // ????? Delete ?????

    [Fact]
    public void Delete_ShouldRemoveCar_WhenExists()
    {
        var car = new Car { Id = 1, Name = "Toyota Fortuner" };

        _mockRepo.Setup(r => r.GetById(1)).Returns(car);
        _mockRepo.Setup(r => r.Delete(car));
        _mockRepo.Setup(r => r.Save());

        _service.Delete(1);

        _mockRepo.Verify(r => r.Delete(car), Times.Once);
        _mockRepo.Verify(r => r.Save(), Times.Once);
    }

    [Fact]
    public void Delete_ShouldThrow_WhenCarNotFound()
    {
        _mockRepo.Setup(r => r.GetById(999)).Returns((Car)null!);

        Action act = () => _service.Delete(999);

        act.Should().Throw<Exception>().WithMessage("Car not found");
    }
}