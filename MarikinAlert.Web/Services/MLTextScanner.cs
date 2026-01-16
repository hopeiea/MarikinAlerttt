namespace MarikinAlert.Web.Services
{
    using Microsoft.ML;
    using MarikinAlert.Web.Models;
    using MarikinAlert.Web.Services.ML;

    /// <summary>
    /// ML-Powered Text Scanner using trained models
    /// Accuracy: Category 88.21%, Priority 81.70%
    /// Runs completely offline on edge node CPU
    /// </summary>
    public class MLTextScanner : ITextScanner
    {
        private readonly PredictionEngine<CategoryInput, CategoryOutput> _categoryEngine;
        private readonly PredictionEngine<PriorityInput, PriorityOutput> _priorityEngine;
        private readonly ITextScanner _fallbackScanner; // Fallback to keyword-based if ML fails

        public MLTextScanner(ITextScanner fallbackScanner)
        {
            _fallbackScanner = fallbackScanner;

            var mlContext = new MLContext(seed: 1);

            try
            {
                // Load Category Model
                string categoryModelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLModels", "CategoryModel.zip");
                var categoryModel = mlContext.Model.Load(categoryModelPath, out var categorySchema);
                _categoryEngine = mlContext.Model.CreatePredictionEngine<CategoryInput, CategoryOutput>(categoryModel);

                // Load Priority Model
                string priorityModelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLModels", "PriorityModel.zip");
                var priorityModel = mlContext.Model.Load(priorityModelPath, out var prioritySchema);
                _priorityEngine = mlContext.Model.CreatePredictionEngine<PriorityInput, PriorityOutput>(priorityModel);

                Console.WriteLine("✅ ML Models loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ ML Model loading failed: {ex.Message}");
                Console.WriteLine("Falling back to keyword-based scanner...");
                throw; // Let dependency injection handle fallback
            }
        }

        public ReportCategory ScanForCategory(string rawMessage)
        {
            Console.WriteLine($"\n[ML-SCANNER] Predicting Category for: '{rawMessage}'...");

            if (string.IsNullOrWhiteSpace(rawMessage))
                return ReportCategory.Noise;

            try
            {
                // Prepare input
                var input = new CategoryInput { RawMessage = rawMessage };

                // Get ML prediction
                var prediction = _categoryEngine.Predict(input);

                // Convert string prediction to enum
                if (Enum.TryParse<ReportCategory>(prediction.PredictedCategory, true, out var category))
                {
                    return category;
                }

                // If parsing fails, use fallback
                return _fallbackScanner.ScanForCategory(rawMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ ML Category prediction failed: {ex.Message}");
                return _fallbackScanner.ScanForCategory(rawMessage);
            }
        }
        public ReportPriority DeterminePriority(string rawMessage, ReportCategory category)
        {
            Console.WriteLine($"[ML-SCANNER] Predicting Priority for: '{rawMessage}'...");
            
            if (string.IsNullOrWhiteSpace(rawMessage))
                return ReportPriority.Low;

            try
            {
                // 1. GET ML PREDICTION (Ask the AI first)
                var input = new PriorityInput { RawMessage = rawMessage };
                var prediction = _priorityEngine.Predict(input);

                ReportPriority finalPriority = ReportPriority.Low; // Default
                
                // Try to parse the ML output
                if (!Enum.TryParse<ReportPriority>(prediction.PredictedPriority, true, out finalPriority))
                {
                    // If ML gives garbage, fallback to the keyword scanner
                    finalPriority = _fallbackScanner.DeterminePriority(rawMessage, category);
                }

                // =================================================================
                // 🛡️ SMART SAFETY LAYER (The "Expert System" Logic)
                // This overrides the AI if it makes a dangerous mistake.
                // =================================================================
                string cleanText = rawMessage.ToLower();

                // A. TRIVIAL OBJECT BLOCKLIST (Negative Filter)
                // If the message is about these small things, TRUST the AI (even if Low).
                string[] trivialObjects = { 
                    "posporo", "kandila", "lighter", "stick", "match", "candle", 
                    "siga", "dahon", "basura", "trash", "bonfire", "usok lang",
                    "niluluto", "sinaing", "ulam", "kalan", "stove"
                };

                bool isTrivial = trivialObjects.Any(obj => cleanText.Contains(obj));

                // B. SAFETY OVERRIDES (Only apply if NOT a trivial object)
                if (!isTrivial)
                {
                    // RULE 1: Fire + Structure/Location = AUTOMATIC CRITICAL
                    if ((cleanText.Contains("sunog") || cleanText.Contains("fire") || cleanText.Contains("apoy")) && 
                        (cleanText.Contains("bahay") || cleanText.Contains("house") || cleanText.Contains("bldg") || cleanText.Contains("building") || cleanText.Contains("paaralan") || cleanText.Contains("palengke")))
                    {
                        return ReportPriority.Critical;
                    }

                    // RULE 2: Fire + Help = AUTOMATIC CRITICAL
                    if ((cleanText.Contains("sunog") || cleanText.Contains("fire") || cleanText.Contains("nasusunog")) && 
                        (cleanText.Contains("tulong") || cleanText.Contains("help") || cleanText.Contains("saklolo")))
                    {
                        return ReportPriority.Critical;
                    }

                    // RULE 3: Life Threats (Injured/Trapped) = AUTOMATIC CRITICAL
                    if (cleanText.Contains("trap") || cleanText.Contains("sugatan") || cleanText.Contains("dugo") || cleanText.Contains("tao sa loob"))
                    {
                        return ReportPriority.Critical;
                    }

                    // RULE 4: The "Vague Fire" Fix (For your specific Malanday case!)
                    // If it says "Sunog" but ML thought it was Low/Medium -> Force HIGH.
                    if ((cleanText.Contains("sunog") || cleanText.Contains("fire") || cleanText.Contains("nasusunog")) &&
                        (finalPriority == ReportPriority.Low || finalPriority == ReportPriority.Medium))
                    {
                        return ReportPriority.High; 
                    }
                }
                
                return finalPriority;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ ML Priority prediction failed: {ex.Message}");
                return _fallbackScanner.DeterminePriority(rawMessage, category);
            }
        }
    }
}