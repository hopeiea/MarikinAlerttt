using System;

namespace MarikinAlert.Web.Models
{
    public class ArchivedReport
    {
        public int Id { get; set; }
        public int OriginalReportId { get; set; }
        
        public string SenderName { get; set; }
        public string ContactNumber { get; set; }
        public string RawMessage { get; set; }
        public string Location { get; set; }
        
        public string Category { get; set; }
        public string Priority { get; set; }
        
        public double ConfidenceScore { get; set; }
        
        public string Status { get; set; }
        
        public DateTime OriginalTimestamp { get; set; }
        public DateTime ArchivedTimestamp { get; set; } = DateTime.Now;
    }
}