using CarRentalAPI.Data;
using CarRentalAPI.Models;
using CarRentalAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.Status != "Deleted")
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByUserIdAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.UserId == userId && b.Status == "Booked")
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<bool> IsCarBooked(int carId, DateTime startDate, DateTime endDate)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.CarId == carId &&
                b.Status == "Booked" &&
                b.StartDate <= endDate &&
                b.EndDate >= startDate);
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings.FindAsync(id);
        }

        public async Task DeleteAsync(Booking booking)
        {
            _context.Bookings.Remove(booking);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}