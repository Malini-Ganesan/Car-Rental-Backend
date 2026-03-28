using CarRentalAPI.Models;
using CarRentalAPI.Services.Interface;
using CarRentalAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

    namespace CarRentalAPI.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class BookingController : ControllerBase
        {
        private readonly IBookingService _bookingService;
        private readonly NodeRedService _nodeRed;
        private readonly ICarService _carService;

        public BookingController(
            IBookingService bookingService,
            NodeRedService nodeRed,
            ICarService carService)
        {
            _bookingService = bookingService;
            _nodeRed = nodeRed;
            _carService = carService;
        }


        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Booking booking)
        {
            try
            {
                booking.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

                booking.UserName = User.FindFirst("preferred_username")?.Value;

                booking.StartDate = booking.StartDate.ToUniversalTime();
                booking.EndDate = booking.EndDate.ToUniversalTime();

                if (booking.CarId <= 0 || string.IsNullOrEmpty(booking.Location))
                    return BadRequest("Invalid booking data");

                var result = await _bookingService.CreateBookingAsync(booking, booking.UserId);

                var car = _carService.GetById(booking.CarId);
                var carName = car?.Name ?? "Unknown Car";

                try
                {
                    await _nodeRed.SendEvent(
                        "BOOKING_CREATED",
                        $"Car {carName} booked at {booking.Location} by {booking.UserName}"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Node-RED error: " + ex.Message);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "User")]
            [HttpGet("my-bookings")]
            public async Task<IActionResult> MyBookings()
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

                var bookings = await _bookingService.GetMyBookingsAsync(userId);

                // Include car info
                var result = bookings.Select(b => new
                {
                    b.Id,
                    CarName = b.Car != null ? b.Car.Name : "Unknown Car",
                    b.StartDate,
                    b.EndDate,
                    b.Location,
                    b.TotalPrice,
                    b.Status
                });

                return Ok(result);
            }

            [Authorize(Roles = "Admin")]
            [HttpGet("all-bookings")]
            public async Task<IActionResult> AllBookings()
            {
                var bookings = await _bookingService.GetAllBookingsAsync();

                // Include car and user info
                var result = bookings.Select(b => new
                {
                    b.Id,
                    CarName = b.Car != null ? b.Car.Name : "Unknown Car",
                    UserName = b.UserName,
                    b.StartDate,
                    b.EndDate,
                    b.Location,
                    b.TotalPrice,
                    b.Status
                });

                return Ok(result);
            }

            [Authorize(Roles = "Admin")]
            [HttpGet]
            public async Task<IActionResult> GetAll()
            {
                var bookings = await _bookingService.GetAllBookingsAsync();

                return Ok(bookings);
            }
    [HttpGet("check-availability")]
    public async Task<IActionResult> CheckAvailability(
        int carId,
        string startDate,
        string endDate)
    {
        if (!DateTime.TryParse(startDate, out DateTime start))
            return BadRequest("Invalid start date");

        if (!DateTime.TryParse(endDate, out DateTime end))
            return BadRequest("Invalid end date");

        var isBooked = await _bookingService.IsCarBooked(carId, start, end);

        return Ok(new { isBooked });
    }
            [Authorize(Roles = "Admin")]
            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(int id)
            {
                try
                {
                    await _bookingService.DeleteBookingAsync(id);
                    return Ok(new { message = "Booking deleted successfully", id = id });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            [Authorize(Roles = "User, Admin")]
[HttpPut("cancel/{id}")]
public async Task<IActionResult> Cancel(int id)
{
    try
    {
        var bookingData = await _bookingService.GetBookingById(id);

        await _bookingService.CancelBookingAsync(id);

        var carName = bookingData?.Car?.Name ?? "Unknown Car";

        try
        {
            await _nodeRed.SendEvent(
                "BOOKING_CANCELLED",
                $"Car {carName} booking cancelled"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("Node-RED error: " + ex.Message);
        }

        return Ok("Booking cancelled");
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
        }
    }