using CarRentalAPI.DTOs;

namespace CarRentalAPI.Services.Interface
{
    public interface ICarService
    {
        IEnumerable<CarDto> GetAll();
        CarDto? GetById(int id);
        CarDto Create(CarCreateDto dto);
        void Update(int id, CarCreateDto dto);
        void Delete(int id);
    }
}
