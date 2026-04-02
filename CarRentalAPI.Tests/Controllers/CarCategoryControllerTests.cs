using CarRentalAPI.Controllers;
using CarRentalAPI.Models;
using CarRentalAPI.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalAPI.Tests.Controllers;

public class CarCategoryControllerTests
{
    [Fact]
    public void GetAll_ShouldReturn200_WithCategories()
    {
        // Arrange — clean DB, add exactly 3
        var context = TestDbContextFactory.Create();
        context.CarCategories.AddRange(
            new CarCategory { Id = 10, Name = "SUV" },
            new CarCategory { Id = 11, Name = "Sedan" },
            new CarCategory { Id = 12, Name = "Hatchback" }
        );
        context.SaveChanges();

        var controller = new CarCategoryController(context);

        // Act
        var result = controller.GetAll();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = ok.Value.Should().BeAssignableTo<List<CarCategory>>().Subject;
        categories.Should().HaveCount(3);
    }

    [Fact]
    public void GetAll_ShouldReturn200_WithEmptyList_WhenNoCategories()
    {
        // Arrange — clean DB with nothing added
        var context = TestDbContextFactory.Create();
        var controller = new CarCategoryController(context);

        // Act
        var result = controller.GetAll();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = ok.Value.Should().BeAssignableTo<List<CarCategory>>().Subject;
        categories.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_ShouldReturnCorrectCategoryNames()
    {
        // Arrange — only one category, so First() is predictable
        var context = TestDbContextFactory.Create();
        context.CarCategories.Add(new CarCategory { Id = 20, Name = "Luxury" });
        context.SaveChanges();

        var controller = new CarCategoryController(context);

        // Act
        var result = controller.GetAll();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = ok.Value.Should().BeAssignableTo<List<CarCategory>>().Subject;
        categories.Should().HaveCount(1);
        categories.First().Name.Should().Be("Luxury");
    }
}