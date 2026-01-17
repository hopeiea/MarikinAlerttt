using Microsoft.ML;
using Microsoft.ML.Data;
using MarikinAlert.Web.Models;
using MarikinAlert.Web.Data;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace MarikinAlert.Web.Services
{
    // FIX 1: The Input MUST match the Trainer's columns exactly, 
    // even if we leave them empty during prediction.
    public class ModelInput
    {
        [LoadColumn(0)] public string Message { get; set; } = string.Empty;
        [LoadColumn(1)] public string Category { get; set; } = string.Empty; // Added this
        [LoadColumn(2)] public string Priority { get; set; } = string.Empty; // Added this
    }

    public class ModelOutput
    {
        [ColumnName("PredictedLabel")] public string Prediction { get; set; } = string.Empty;
        public float[] Score { get; set; } = Array.Empty<float>();
    }

    public class AiTriageService : IDisasterTriageService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MLContext _mlContext;
        private PredictionEngine<ModelInput, ModelOutput>? _categoryEngine;
        private PredictionEngine<ModelInput, ModelOutput>? _priorityEngine;

        public AiTriageService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _mlContext = new MLContext();

            // Paths to your .zip files
            string categoryPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModels", "CategoryModel.zip");
            string priorityPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModels", "PriorityModel.zip");

            // Load Category Model
            if (File.Exists(categoryPath))
            {
                // We define the input schema explicitly to satisfy the model's strict requirements
                ITransformer categoryModel = _mlContext.Model.Load(categoryPath, out var _);
                _categoryEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(categoryModel);
            }

            // Load Priority Model
            if (File.Exists(priorityPath))
            {
                ITransformer priorityModel = _mlContext.Model.Load(priorityPath, out var _);
                _priorityEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(priorityModel);
            }
        }

        public async Task<DisasterReport> TriageAndAnalyzeAsync(string message, string sender, string contact, string location)
        {
            // FIX 2: Create input with Dummy values for Category/Priority to satisfy the schema
            var input = new ModelInput
            {
                Message = message,
                Category = "Fire", // The model ignores this during prediction, but needs the column to exist
                Priority = "High"  // The model ignores this during prediction, but needs the column to exist
            };

            string categoryPred = "Unknown";
            string priorityPred = "Low";

            // Predict
            if (_categoryEngine != null) categoryPred = _categoryEngine.Predict(input).Prediction;
            if (_priorityEngine != null) priorityPred = _priorityEngine.Predict(input).Prediction;

            // Parse Enums (with fallbacks)
            Enum.TryParse(categoryPred, true, out ReportCategory finalCategory);
            Enum.TryParse(priorityPred, true, out ReportPriority finalPriority);

            var report = new DisasterReport
            {
                // If your database has a 'Timestamp' or 'Date' field, uncomment the next line:
                // ReportTime = DateTime.Now, 

                RawMessage = message,
                SenderName = sender,
                ContactNumber = contact,
                Location = location,
                Category = finalCategory,
                Priority = finalPriority,
                Status = ReportStatus.Pending,
                ConfidenceScore = 0.90
            };

            // Save to DB
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IDisasterRepository>();
                // Ensure this matches your Repository method name (AddAsync, CreateReport, etc.)
                await repo.AddAsync(report);
            }

            return report;
        }

        public async Task<DisasterReport?> GetReportByIdAsync(int id)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IDisasterRepository>();
                return await repo.GetByIdAsync(id);
            }
        }

        public async Task<IEnumerable<DisasterReport>> GetAllReportsAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IDisasterRepository>();
                return await repo.GetAllAsync();
            }
        }
    }
}