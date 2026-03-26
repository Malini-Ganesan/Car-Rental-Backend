using CarRentalAPI.Repositories.Interface;
using CarRentalAPI.Services.Interfaces;

namespace CarRentalAPI.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ICarRepository _carRepo;
        private readonly IBookingRepository _bookingRepo;

        public DashboardService(ICarRepository carRepo, IBookingRepository bookingRepo)
        {
            _carRepo = carRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<object> GetDashboardDataAsync()
        {
            var cars = await _carRepo.GetAllAsync();
            var bookings = await _bookingRepo.GetAllAsync();

            var totalCars = cars.Count();
            var totalBookings = bookings.Count();

            var bookingsPerCar = cars.Select(car => new
            {
                car.Name,
                BookingCount = bookings.Count(b => b.CarId == car.Id)
            });

            return new
            {
                totalCars,
                totalBookings,
                bookingsPerCar
            };
        }
    }
}