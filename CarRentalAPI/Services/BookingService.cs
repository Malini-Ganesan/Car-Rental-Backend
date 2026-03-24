using CarRentalAPI.Models;
using CarRentalAPI.Repositories.Interface;
using CarRentalAPI.Services.Interfaces;

namespace CarRentalAPI.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;

        public BookingService(IBookingRepository bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking, string userId)
        {
            booking.UserId = userId;

            bool isBooked = await _bookingRepo.IsCarBooked(
                booking.CarId,
                booking.StartDate,
                booking.EndDate
            );

            if (isBooked)
                throw new Exception("Car already booked for selected dates.");

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveAsync();

            return booking;
        }

        public async Task<IEnumerable<Booking>> GetMyBookingsAsync(string userId)
        {
            return await _bookingRepo.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _bookingRepo.GetAllAsync();
        }

        public async Task<bool> IsCarBooked(int carId, DateTime startDate, DateTime endDate)
        {
            return await _bookingRepo.IsCarBooked(carId, startDate, endDate);
        }

        public async Task DeleteBookingAsync(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);

            if (booking == null)
                throw new Exception("Booking not found");

            await _bookingRepo.DeleteAsync(booking);
            await _bookingRepo.SaveAsync();
        }

        public async Task CancelBookingAsync(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);

            if (booking == null)
                throw new Exception("Booking not found");

            booking.Status = "Cancelled"; 

            await _bookingRepo.SaveAsync();
        }
    }
}