namespace CarRentalAPI.DTOs
{
    public class CarDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public string InsuranceName { get; set; } = string.Empty;
        public decimal InsuranceCost { get; set; }
        public string? ImageUrl { get; set; }
    }
}
