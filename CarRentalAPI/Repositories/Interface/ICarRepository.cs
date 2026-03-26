using CarRentalAPI.Models;

namespace CarRentalAPI.Repositories.Interface
{
    public interface ICarRepository
    {
        IEnumerable<Car> GetAll();
        Car? GetById(int id);
        void Add(Car car);
        void Update(Car car);
        void Delete(Car car);
        void Save();
        Task<IEnumerable<Car>> GetAllAsync();
    }
}
