using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Tests.Helpers;

public static class TestDbContextFactory
{
    // ?? Clean DB with NO seed data ??
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
            .Options;

        return new ApplicationDbContext(options);
    }

    // ?? DB with seed data for service tests ??
    public static ApplicationDbContext CreateWithSeedData()
    {
        var context = Create();

        context.InsurancePlans.Add(new InsurancePlan
        { Id = 1, Name = "Basic", CostPerDay = 10 });

        context.CarCategories.Add(new CarCategory
        { Id = 1, Name = "SUV" });

        context.Cars.Add(new Car
        {
            Id = 1,
            Name = "Toyota Fortuner",
            CategoryId = 1,
            InsurancePlanId = 1,
            PricePerHour = 10,
            PricePerDay = 50,
            PricePerWeek = 300,
            CreatedAt = DateTime.UtcNow
        });

        context.SaveChanges();
        return context;
    }
}