using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalAPI.Data;
using CarRentalAPI.Models;

[Route("api/[controller]")]
[ApiController]
public class LogController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LogController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _context.SystemLogs
            .Where(x => x.EventType != null)
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .ToListAsync();

        return Ok(logs);
    }
}