namespace CarRentalAPI.DTOs
{
    public class CarCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal PricePerHour { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public int InsurancePlanId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
