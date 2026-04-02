using CarRentalAPI.DTOs;
using CarRentalAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRentalAPI.Models;
using CarRentalAPI.Services;

namespace CarRentalAPI.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CarController : ControllerBase
{
    private readonly ICarService _carService;
    private readonly NodeRedService _nodeRed;


    public CarController(ICarService carService, NodeRedService nodeRed)
    {
        _carService = carService;
        _nodeRed = nodeRed;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetAll() => Ok(_carService.GetAll());

    [HttpGet("{id}")]
    [AllowAnonymous]
    public IActionResult GetById(int id)
    {
        var car = _carService.GetById(id);
        if (car == null) return NotFound();
        return Ok(car);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CarCreateDto dto)
    {
        var car = _carService.Create(dto);
        try
        {
            await _nodeRed.SendEvent(
                "CAR_CREATED",
                $"Car {car.Name} created"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("Node-RED failed: " + ex.Message);
        }

        return Ok(car);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromForm] CarCreateDto dto)
    {
        _carService.Update(id, dto);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _carService.Delete(id);
        return NoContent();
    }
}