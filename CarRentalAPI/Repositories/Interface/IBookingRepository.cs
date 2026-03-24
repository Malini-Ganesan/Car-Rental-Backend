using CarRentalAPI.Models;

namespace CarRentalAPI.Repositories.Interface
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<IEnumerable<Booking>> GetByUserIdAsync(string userId);
        Task<bool> IsCarBooked(int carId, DateTime startDate, DateTime endDate);
        Task AddAsync(Booking booking);
        Task DeleteAsync(Booking booking);
        Task<Booking?> GetByIdAsync(int id);
        Task SaveAsync();
    }
}