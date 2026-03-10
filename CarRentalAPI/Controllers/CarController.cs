using CarRentalAPI.DTOs;
using CarRentalAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRentalAPI.Models;

namespace CarRentalAPI.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CarController : ControllerBase
{
    private readonly ICarService _carService;

    public CarController(ICarService carService)
    {
        _carService = carService;
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
    public IActionResult Create([FromForm] CarCreateDto dto)
    {
        var car = _carService.Create(dto);
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