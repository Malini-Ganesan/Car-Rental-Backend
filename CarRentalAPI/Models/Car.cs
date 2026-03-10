
using System.ComponentModel.DataAnnotations.Schema;
namespace CarRentalAPI.Models
{

    [Table("Cars")]
    public class Car
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public CarCategory? Category { get; set; }

        public decimal PricePerHour { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }

        public int InsurancePlanId { get; set; }
        public InsurancePlan? InsurancePlan { get; set; }

        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
