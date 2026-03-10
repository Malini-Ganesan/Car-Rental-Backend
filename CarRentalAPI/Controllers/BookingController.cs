using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Booking booking)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            booking.UserId = userId;
            booking.StartDate = DateTime.SpecifyKind(booking.StartDate, DateTimeKind.Utc);
            booking.EndDate = DateTime.SpecifyKind(booking.EndDate, DateTimeKind.Utc);

            var overlapping = await _context.Bookings.AnyAsync(b =>
                b.CarId == booking.CarId &&
                b.StartDate <= booking.EndDate &&
                b.EndDate >= booking.StartDate);

            if (overlapping)
                return BadRequest("Car already booked for selected dates.");

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(booking);
        }
    }
}