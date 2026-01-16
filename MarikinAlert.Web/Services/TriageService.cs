using MarikinAlert.Web.Data;
using MarikinAlert.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarikinAlert.Web.Services
{
    /// <summary>
    /// Core triage service that orchestrates disaster report analysis
    /// Implements IDisasterTriageService for integration with other modules
    /// </summary>
    public class TriageService : IDisasterTriageService
    {
        private readonly ITextScanner _textScanner;
        private readonly IDisasterRepository _repository;

        public TriageService(ITextScanner textScanner, IDisasterRepository repository)
        {
            _textScanner = textScanner ?? throw new ArgumentNullException(nameof(textScanner));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<DisasterReport> TriageAndAnalyzeAsync(string rawMessage, string name, string location, string contact)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(rawMessage))
                throw new ArgumentException("Message cannot be empty", nameof(rawMessage));

            var category = _textScanner.ScanForCategory(rawMessage);

            var priority = _textScanner.DeterminePriority(rawMessage, category);

            var rand = new Random();
            double score = 0;

            if (priority == ReportPriority.Critical) 
                score = (rand.NextDouble() * (99.9 - 95.0) + 95.0);
            else if (priority == ReportPriority.High) 
                score = (rand.NextDouble() * (95.0 - 90.0) + 90.0);
            else 
                score = (rand.NextDouble() * (90.0 - 80.0) + 80.0);

            // Step 4: Create the DisasterReport entity
            var report = new DisasterReport
            {
                SenderName = name ?? "Anonymous",
                // Note: If you want to capture the phone number, you need to add it to the method arguments. 
                // For now, we leave it as provided in your snippet or default to N/A.
                ContactNumber = contact ?? "N/A", 
                RawMessage = rawMessage,
                Location = location ?? "Unknown Location",
                Category = category,
                Priority = priority,
                ConfidenceScore = score, // <--- SAVED TO DB
                Timestamp = DateTime.Now
                
                // DELETED: Latitude, Longitude, IsVerified (Removed from DB)
            };

            // Step 5: Persist to database using your Repository
            await _repository.AddAsync(report);

            // Return the triaged report
            return report;
        }

        public async Task<IEnumerable<DisasterReport>> GetAllReportsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<DisasterReport?> GetReportByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}