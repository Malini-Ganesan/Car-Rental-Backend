using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalAPI.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class InsurancePlanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InsurancePlanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var plans = _context.InsurancePlans.ToList();
            return Ok(plans);
        }
    }
}