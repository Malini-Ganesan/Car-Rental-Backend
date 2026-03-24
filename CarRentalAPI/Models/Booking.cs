using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CarRentalAPI.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int CarId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public string? Location { get; set; }
        [Required]
        public decimal TotalPrice { get; set; }

        [ForeignKey("CarId")]
        [JsonIgnore]
        public Car? Car { get; set; }
      
        public string Status { get; set; } = "Booked";
    }
}