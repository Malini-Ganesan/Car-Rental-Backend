using CarRentalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<CarCategory> CarCategories { get; set; }
    public DbSet<InsurancePlan> InsurancePlans { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    public DbSet<SystemLog> SystemLogs { get; set; }


}