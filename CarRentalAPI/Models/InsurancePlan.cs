using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalAPI.Models
{
    [Table("InsurancePlan")]
    public class InsurancePlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CostPerDay { get; set; }
        public ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
