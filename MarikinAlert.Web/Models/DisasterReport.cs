using System;

namespace MarikinAlert.Web.Models
{
    public enum ReportCategory { Fire, BuildingCollapse, Medical, Logistics, Infrastructure, Noise }
    public enum ReportPriority { Critical, High, Medium, Low }

    public class DisasterReport
    {
        public int Id { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string RawMessage { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        
        public ReportCategory Category { get; set; }
        public ReportPriority Priority { get; set; }
        public double ConfidenceScore { get; set; } 
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}