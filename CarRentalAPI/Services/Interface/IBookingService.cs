using CarRentalAPI.Models;

namespace CarRentalAPI.Services.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(Booking booking, string userId);
        Task<IEnumerable<Booking>> GetMyBookingsAsync(string userId);
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<bool> IsCarBooked(int carId, DateTime startDate, DateTime endDate);
        Task DeleteBookingAsync(int id);
        Task CancelBookingAsync(int id);
        Task<Booking> GetBookingById(int id);
    }
}