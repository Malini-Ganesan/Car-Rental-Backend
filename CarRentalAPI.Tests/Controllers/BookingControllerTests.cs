using CarRentalAPI.Controllers;
using CarRentalAPI.Models;
using CarRentalAPI.Services;
using CarRentalAPI.Services.Interface;
using CarRentalAPI.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CarRentalAPI.Tests.Controllers;

public class BookingControllerTests
{
	private readonly Mock<IBookingService> _mockBookingService;
	private readonly Mock<ICarService> _mockCarService;
	private readonly BookingController _controller;

	public BookingControllerTests()
	{
		_mockBookingService = new Mock<IBookingService>();
		_mockCarService = new Mock<ICarService>();

		// NodeRedService is NOT mocked — use real instance with fake HTTP handler
		// Controller wraps all NodeRed calls in try/catch so failures are silent
		var fakeHttpClient = new HttpClient(new FakeHttpMessageHandler());
		var nodeRed = new NodeRedService(fakeHttpClient);

		_controller = new BookingController(
			_mockBookingService.Object,
			nodeRed,
			_mockCarService.Object
		);

		SetUser("user1", "John", "User");
	}

	private void SetUser(string userId, string username, string role)
	{
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, userId),
			new Claim("preferred_username", username),
			new Claim(ClaimTypes.Role, role)
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var principal = new ClaimsPrincipal(identity);

		_controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext { User = principal }
		};
	}

	// ????? POST /api/booking ?????

	[Fact]
	public async Task Create_ShouldReturn200_WhenBookingIsValid()
	{
		// Arrange
		var booking = new Booking
		{
			CarId = 1,
			StartDate = DateTime.UtcNow,
			EndDate = DateTime.UtcNow.AddDays(2),
			Location = "Chennai",
			TotalPrice = 100
		};

		_mockBookingService
			.Setup(s => s.CreateBookingAsync(It.IsAny<Booking>(), It.IsAny<string>()))
			.ReturnsAsync(booking);

		_mockCarService
			.Setup(s => s.GetById(1))
			.Returns(new CarRentalAPI.DTOs.CarDto { Id = 1, Name = "Toyota Fortuner" });

		// Act
		var result = await _controller.Create(booking);

		// Assert
		result.Should().BeOfType<OkObjectResult>();
	}

	[Fact]
	public async Task Create_ShouldReturn400_WhenCarIdIsZero()
	{
		var booking = new Booking
		{
			CarId = 0,
			Location = "Chennai",
			StartDate = DateTime.UtcNow,
			EndDate = DateTime.UtcNow.AddDays(1),
			TotalPrice = 50
		};

		var result = await _controller.Create(booking);

		result.Should().BeOfType<BadRequestObjectResult>()
			  .Which.Value.Should().Be("Invalid booking data");
	}

	[Fact]
	public async Task Create_ShouldReturn400_WhenLocationIsEmpty()
	{
		var booking = new Booking
		{
			CarId = 1,
			Location = "",
			StartDate = DateTime.UtcNow,
			EndDate = DateTime.UtcNow.AddDays(1),
			TotalPrice = 50
		};

		var result = await _controller.Create(booking);

		result.Should().BeOfType<BadRequestObjectResult>()
			  .Which.Value.Should().Be("Invalid booking data");
	}

	// ????? GET /api/booking/my-bookings ?????

	[Fact]
	public async Task MyBookings_ShouldReturn200_WithUserBookings()
	{
		var bookings = new List<Booking>
		{
			new Booking
			{
				Id         = 1,
				UserId     = "user1",
				CarId      = 1,
				Car        = new Car { Name = "Toyota Fortuner" },
				StartDate  = DateTime.UtcNow,
				EndDate    = DateTime.UtcNow.AddDays(2),
				Location   = "Chennai",
				TotalPrice = 100,
				Status     = "Booked"
			}
		};

		_mockBookingService
			.Setup(s => s.GetMyBookingsAsync("user1"))
			.ReturnsAsync(bookings);

		var result = await _controller.MyBookings();

		result.Should().BeOfType<OkObjectResult>();
	}

	// ????? GET /api/booking/all-bookings ?????

	[Fact]
	public async Task AllBookings_ShouldReturn200_WithAllBookings()
	{
		var bookings = new List<Booking>
		{
			new Booking
			{
				Id         = 1,
				UserId     = "user1",
				CarId      = 1,
				Car        = new Car { Name = "Toyota Fortuner" },
				UserName   = "John",
				StartDate  = DateTime.UtcNow,
				EndDate    = DateTime.UtcNow.AddDays(2),
				Location   = "Chennai",
				TotalPrice = 100,
				Status     = "Booked"
			}
		};

		_mockBookingService
			.Setup(s => s.GetAllBookingsAsync())
			.ReturnsAsync(bookings);

		var result = await _controller.AllBookings();

		result.Should().BeOfType<OkObjectResult>();
	}

	// ????? GET /api/booking/check-availability ?????

	[Fact]
	public async Task CheckAvailability_ShouldReturnIsBooked_True()
	{
		_mockBookingService
			.Setup(s => s.IsCarBooked(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
			.ReturnsAsync(true);

		var result = await _controller.CheckAvailability(1, "2025-06-01", "2025-06-05");

		var ok = result.Should().BeOfType<OkObjectResult>().Subject;
		var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
		json.Should().Contain("true");
	}

	[Fact]
	public async Task CheckAvailability_ShouldReturn400_WhenStartDateIsInvalid()
	{
		var result = await _controller.CheckAvailability(1, "not-a-date", "2025-06-05");

		result.Should().BeOfType<BadRequestObjectResult>()
			  .Which.Value.Should().Be("Invalid start date");
	}

	[Fact]
	public async Task CheckAvailability_ShouldReturn400_WhenEndDateIsInvalid()
	{
		var result = await _controller.CheckAvailability(1, "2025-06-01", "not-a-date");

		result.Should().BeOfType<BadRequestObjectResult>()
			  .Which.Value.Should().Be("Invalid end date");
	}

	// ????? DELETE /api/booking/{id} ?????

	[Fact]
	public async Task Delete_ShouldReturn200_WhenSuccessful()
	{
		_mockBookingService
			.Setup(s => s.DeleteBookingAsync(1))
			.Returns(Task.CompletedTask);

		var result = await _controller.Delete(1);

		result.Should().BeOfType<OkObjectResult>();
	}

	[Fact]
	public async Task Delete_ShouldReturn400_WhenBookingNotCancelled()
	{
		_mockBookingService
			.Setup(s => s.DeleteBookingAsync(1))
			.ThrowsAsync(new Exception("Only cancelled bookings can be deleted"));

		var result = await _controller.Delete(1);

		result.Should().BeOfType<BadRequestObjectResult>();
	}

	// ????? PUT /api/booking/cancel/{id} ?????

	[Fact]
	public async Task Cancel_ShouldReturn200_WhenSuccessful()
	{
		// Arrange
		var booking = new Booking
		{
			Id = 1,
			CarId = 1,
			Car = new Car { Name = "Toyota Fortuner" },
			Location = "Chennai",
			TotalPrice = 50,
			Status = "Booked"
		};

		_mockBookingService
			.Setup(s => s.GetBookingById(1))
			.ReturnsAsync(booking);

		_mockBookingService
			.Setup(s => s.CancelBookingAsync(1))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _controller.Cancel(1);

		// Assert
		result.Should().BeOfType<OkObjectResult>();
	}

	[Fact]
	public async Task Cancel_ShouldReturn400_WhenBookingNotFound()
	{
		_mockBookingService
			.Setup(s => s.GetBookingById(1))
			.ReturnsAsync((Booking)null!);

		_mockBookingService
			.Setup(s => s.CancelBookingAsync(1))
			.ThrowsAsync(new Exception("Booking not found"));

		var result = await _controller.Cancel(1);

		result.Should().BeOfType<BadRequestObjectResult>();
	}
}

// ?? Fake HTTP handler so NodeRedService doesn't make real network calls ??
public class FakeHttpMessageHandler : HttpMessageHandler
{
	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		return Task.FromResult(
			new HttpResponseMessage(System.Net.HttpStatusCode.OK)
		);
	}
}