using CarRentalAPI.Models;
using CarRentalAPI.Repositories;
using CarRentalAPI.DTOs;
using CarRentalAPI.Repositories.Interface;
using CarRentalAPI.Services.Interface;

namespace CarRentalAPI.Services {
public class CarService : ICarService
{
    private readonly ICarRepository _repo;
    private readonly IWebHostEnvironment _env;

    public CarService(ICarRepository repo, IWebHostEnvironment env)
    {
        _repo = repo;
        _env = env;
    }

    public IEnumerable<CarDto> GetAll()
    {
        return _repo.GetAll().Select(c => new CarDto
        {
            Id = c.Id,
            Name = c.Name,
            CategoryName = c.Category?.Name ?? "",
            PricePerHour = c.PricePerHour,
            PricePerDay = c.PricePerDay,
            PricePerWeek = c.PricePerWeek,
            InsuranceName = c.InsurancePlan?.Name ?? "",
            InsuranceCost = c.InsurancePlan?.CostPerDay ?? 0,
            ImageUrl = c.ImageUrl
        });
    }

    public CarDto? GetById(int id)
    {
        var c = _repo.GetById(id);
        if (c == null) return null;

        return new CarDto
        {
            Id = c.Id,
            Name = c.Name,
            CategoryName = c.Category?.Name ?? "",
            PricePerHour = c.PricePerHour,
            PricePerDay = c.PricePerDay,
            PricePerWeek = c.PricePerWeek,
            InsuranceName = c.InsurancePlan?.Name ?? "",
            InsuranceCost = c.InsurancePlan?.CostPerDay ?? 0,
            ImageUrl = c.ImageUrl
        };
    }

    public CarDto Create(CarCreateDto dto)
    {
        string? imageUrl = null;
        if (dto.Image != null)
        {
            var fileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
            var filePath = Path.Combine(_env.WebRootPath, "images/cars", fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            dto.Image.CopyTo(stream);
            imageUrl = $"/images/cars/{fileName}";
        }

        var car = new Car
        {
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            PricePerHour = dto.PricePerHour,
            PricePerDay = dto.PricePerDay,  
            PricePerWeek = dto.PricePerWeek,
            InsurancePlanId = dto.InsurancePlanId,
            ImageUrl = imageUrl
        };

        _repo.Add(car);
        _repo.Save();

        return GetById(car.Id)!;
    }

    public void Update(int id, CarCreateDto dto)
    {
        var car = _repo.GetById(id);
        if (car == null) throw new Exception("Car not found");

        car.Name = dto.Name;
        car.CategoryId = dto.CategoryId;
        car.PricePerHour = dto.PricePerHour;
        car.PricePerDay = dto.PricePerDay;
        car.PricePerWeek = dto.PricePerWeek;
        car.InsurancePlanId = dto.InsurancePlanId;

        if (dto.Image != null)
        {
            var fileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
            var filePath = Path.Combine(_env.WebRootPath, "images", "cars", fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            dto.Image.CopyTo(stream);
            car.ImageUrl = $"/images/cars/{fileName}";
        }

        _repo.Update(car);
        _repo.Save();
    }

    public void Delete(int id)
    {
        var car = _repo.GetById(id);
        if (car == null) throw new Exception("Car not found");

        _repo.Delete(car);
        _repo.Save();
    }
}
}