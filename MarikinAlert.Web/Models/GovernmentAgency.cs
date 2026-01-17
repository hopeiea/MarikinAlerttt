using System.ComponentModel.DataAnnotations;

namespace MarikinAlert.Web.Models
{
    public class GovernmentAgency
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string Acronym { get; set; } = string.Empty; // e.g., BFP, PNP, MMDA

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(20)]
        public string EmergencyHotline { get; set; } = string.Empty;

        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        // Categories this agency handles (comma-separated or JSON in real implementation)
        [StringLength(200)]
        public string HandlesCategories { get; set; } = string.Empty; // "Fire,Medical"

        public bool IsActive { get; set; } = true;

        public int Priority { get; set; } = 1; // Lower number = higher priority for display
    }
}
