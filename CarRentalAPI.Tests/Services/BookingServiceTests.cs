using CarRentalAPI.Models;
using CarRentalAPI.Repositories.Interface;
using CarRentalAPI.Services;
using FluentAssertions;
using Moq;

namespace CarRentalAPI.Tests.Services;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _mockRepo;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _mockRepo = new Mock<IBookingRepository>();
        _service = new BookingService(_mockRepo.Object);
    }

    // ????? CreateBookingAsync ?????

    [Fact]
    public async Task CreateBooking_ShouldSucceed_WhenCarIsAvailable()
    {
        // Arrange
        var booking = new Booking
        {
            CarId = 1,
            UserId = "user1",
            UserName = "John",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(3),
            Location = "Chennai",
            TotalPrice = 150
        };

        _mockRepo.Setup(r => r.IsCarBooked(1, booking.StartDate, booking.EndDate))
                 .ReturnsAsync(false);
        _mockRepo.Setup(r => r.AddAsync(booking)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateBookingAsync(booking, "user1");

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be("user1");
        result.Status.Should().Be("Booked");
        _mockRepo.Verify(r => r.AddAsync(booking), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBooking_ShouldThrow_WhenCarAlreadyBooked()
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

        _mockRepo.Setup(r => r.IsCarBooked(1, booking.StartDate, booking.EndDate))
                 .ReturnsAsync(true); // already booked

        // Act
        Func<Task> act = async () => await _service.CreateBookingAsync(booking, "user1");

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Car already booked for selected dates.");
    }

    // ????? GetMyBookingsAsync ?????

    [Fact]
    public async Task GetMyBookings_ShouldReturnOnlyUserBookings()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, UserId = "user1", CarId = 1, Location = "Chennai", TotalPrice = 50 },
            new Booking { Id = 2, UserId = "user1", CarId = 2, Location = "Mumbai", TotalPrice = 80 }
        };

        _mockRepo.Setup(r => r.GetByUserIdAsync("user1"))
                 .ReturnsAsync(bookings);

        // Act
        var result = await _service.GetMyBookingsAsync("user1");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.UserId == "user1");
    }

    [Fact]
    public async Task GetMyBookings_ShouldReturnEmpty_WhenNoBookings()
    {
        _mockRepo.Setup(r => r.GetByUserIdAsync("user99"))
                 .ReturnsAsync(new List<Booking>());

        var result = await _service.GetMyBookingsAsync("user99");

        result.Should().BeEmpty();
    }

    // ????? GetAllBookingsAsync ?????

    [Fact]
    public async Task GetAllBookings_ShouldReturnAll()
    {
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, UserId = "user1", CarId = 1, Location = "Chennai", TotalPrice = 50 },
            new Booking { Id = 2, UserId = "user2", CarId = 2, Location = "Delhi",   TotalPrice = 90 }
        };

        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);

        var result = await _service.GetAllBookingsAsync();

        result.Should().HaveCount(2);
    }

    // ????? GetBookingById ?????

    [Fact]
    public async Task GetBookingById_ShouldReturnBooking_WhenExists()
    {
        var booking = new Booking { Id = 5, UserId = "user1", CarId = 1, Location = "Chennai", TotalPrice = 50 };

        _mockRepo.Setup(r => r.GetBookingById(5)).ReturnsAsync(booking);

        var result = await _service.GetBookingById(5);

        result.Should().NotBeNull();
        result.Id.Should().Be(5);
    }

    [Fact]
    public async Task GetBookingById_ShouldReturnNull_WhenNotFound()
    {
        _mockRepo.Setup(r => r.GetBookingById(999)).ReturnsAsync((Booking)null!);

        var result = await _service.GetBookingById(999);

        result.Should().BeNull();
    }

    // ????? IsCarBooked ?????

    [Fact]
    public async Task IsCarBooked_ShouldReturnTrue_WhenOverlapping()
    {
        _mockRepo.Setup(r => r.IsCarBooked(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                 .ReturnsAsync(true);

        var result = await _service.IsCarBooked(1, DateTime.UtcNow, DateTime.UtcNow.AddDays(2));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCarBooked_ShouldReturnFalse_WhenAvailable()
    {
        _mockRepo.Setup(r => r.IsCarBooked(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                 .ReturnsAsync(false);

        var result = await _service.IsCarBooked(1, DateTime.UtcNow, DateTime.UtcNow.AddDays(2));

        result.Should().BeFalse();
    }

    // ????? CancelBookingAsync ?????

    [Fact]
    public async Task CancelBooking_ShouldSetStatusCancelled_WhenBookingExists()
    {
        var booking = new Booking
        {
            Id = 1,
            UserId = "user1",
            CarId = 1,
            Location = "Chennai",
            TotalPrice = 50,
            Status = "Booked"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        await _service.CancelBookingAsync(1);

        booking.Status.Should().Be("Cancelled");
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelBooking_ShouldThrow_WhenBookingNotFound()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Booking)null!);

        Func<Task> act = async () => await _service.CancelBookingAsync(999);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Booking not found");
    }

    // ????? DeleteBookingAsync ?????

    [Fact]
    public async Task DeleteBooking_ShouldSucceed_WhenStatusIsCancelled()
    {
        var booking = new Booking
        {
            Id = 1,
            UserId = "user1",
            CarId = 1,
            Location = "Chennai",
            TotalPrice = 50,
            Status = "Cancelled"  // only cancelled can be deleted
        };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockRepo.Setup(r => r.DeleteAsync(booking)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        await _service.DeleteBookingAsync(1);

        _mockRepo.Verify(r => r.DeleteAsync(booking), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteBooking_ShouldThrow_WhenStatusIsNotCancelled()
    {
        var booking = new Booking
        {
            Id = 1,
            Status = "Booked",  // not cancelled
            Location = "Chennai",
            TotalPrice = 50
        };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        Func<Task> act = async () => await _service.DeleteBookingAsync(1);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Only cancelled bookings can be deleted");
    }

    [Fact]
    public async Task DeleteBooking_ShouldThrow_WhenNotFound()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Booking)null!);

        Func<Task> act = async () => await _service.DeleteBookingAsync(999);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Booking not found");
    }
}