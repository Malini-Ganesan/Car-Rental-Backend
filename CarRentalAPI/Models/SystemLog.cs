using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CarRentalAPI.Models
{
    [Table("system_logs")]
    public class SystemLog
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("event_type")]
        public string? EventType { get; set; }

        [Column("message")]
        public string? Message { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
