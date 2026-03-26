using CarRentalAPI.Data;
using CarRentalAPI.Models;
using CarRentalAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Repositories
{
    public class CarRepository : ICarRepository
    {
        private readonly ApplicationDbContext _context;

        public CarRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Car>> GetAllAsync()
        {
            return await _context.Cars.ToListAsync();
        }
        public IEnumerable<Car> GetAll()
        {
            return _context.Cars.Include(c => c.Category)
                                .Include(c => c.InsurancePlan)
                                .ToList();
        }

        public Car? GetById(int id)
        {
            return _context.Cars.Include(c => c.Category)
                                .Include(c => c.InsurancePlan)
                                .FirstOrDefault(c => c.Id == id);
        }

        public void Add(Car car) => _context.Cars.Add(car);
        public void Update(Car car)
        {
            car.CreatedAt = DateTime.SpecifyKind(car.CreatedAt, DateTimeKind.Utc);
            _context.Cars.Update(car);
        }
        public void Delete(Car car) => _context.Cars.Remove(car);
        public void Save() => _context.SaveChanges();
    }
}