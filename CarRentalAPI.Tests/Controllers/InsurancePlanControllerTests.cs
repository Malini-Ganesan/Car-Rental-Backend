using CarRentalAPI.Controllers;
using CarRentalAPI.Models;
using CarRentalAPI.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalAPI.Tests.Controllers;

public class InsurancePlanControllerTests
{
    // InsurancePlanController uses DbContext directly (no service layer)
    // so we use InMemory database from TestDbContextFactory

    // ????? GET /api/insuranceplan ?????

    [Fact]
    public void GetAll_ShouldReturn200_WithInsurancePlans()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        context.InsurancePlans.AddRange(
            new InsurancePlan { Id = 10, Name = "Basic", CostPerDay = 10 },
            new InsurancePlan { Id = 11, Name = "Standard", CostPerDay = 20 },
            new InsurancePlan { Id = 12, Name = "Premium", CostPerDay = 35 }
        );
        context.SaveChanges();

        var controller = new InsurancePlanController(context);

        // Act
        var result = controller.GetAll();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var plans = ok.Value.Should().BeAssignableTo<List<InsurancePlan>>().Subject;
        plans.Should().HaveCount(3);
    }

    [Fact]
    public void GetAll_ShouldReturn200_WithEmptyList_WhenNoPlans()
    {
        // Arrange — fresh empty DB
        var context = TestDbContextFactory.Create();
        var controller = new InsurancePlanController(context);

        // Act
        var result = controller.GetAll();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var plans = ok.Value.Should().BeAssignableTo<List<InsurancePlan>>().Subject;
        plans.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_ShouldReturnCorrectPlanNames()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        context.InsurancePlans.Add(new InsurancePlan { Id = 20, Name = "Gold", CostPerDay = 50 });
        context.SaveChanges();

        var controller = new InsurancePlanController(context);

        // Act
        var result = controller.GetAll();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var plans = ok.Value.Should().BeAssignableTo<List<InsurancePlan>>().Subject;
        plans.First().Name.Should().Be("Gold");
        plans.First().CostPerDay.Should().Be(50);
    }
}