using CarRentalAPI.Data;
using Microsoft.AspNetCore.Mvc;
using CarRentalAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace CarRentalAPI.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class CarCategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CarCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _context.CarCategories.ToList();
            return Ok(categories);
        }
    }
}
