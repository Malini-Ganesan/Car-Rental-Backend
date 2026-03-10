using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalAPI.Models
{
    [Table("CarCategory")]
    public class CarCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
