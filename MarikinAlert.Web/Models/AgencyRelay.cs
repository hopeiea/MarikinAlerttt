using System;
using System.ComponentModel.DataAnnotations;

namespace MarikinAlert.Web.Models
{
    public class AgencyRelay
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public int AgencyId { get; set; }

        [Required]
        public string RelayedBy { get; set; } = string.Empty; // Admin username

        public DateTime RelayedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Acknowledged, Responding, Completed

        // Reference to the report (navigation property will be added in DbContext)
        // Reference to the agency (navigation property will be added in DbContext)
    }
}
