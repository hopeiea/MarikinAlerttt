namespace MarikinAlert.Web.Services.ML
{
    using Microsoft.ML.Data;

    /// <summary>
    /// Input data structure for ML models
    /// Must match the schema you used during training
    /// </summary>
    public class CategoryInput
    {
        // --- FIX IS HERE ---
        // We map "RawMessage" property to the "Message" column the model expects.
        [ColumnName("Message"), LoadColumn(0)] 
        public string RawMessage { get; set; }

        [ColumnName("Category"), LoadColumn(1)]
        public string Category { get; set; }
    }

    public class CategoryOutput
    {
        [ColumnName("PredictedLabel")]
        public string PredictedCategory { get; set; }

        [ColumnName("Score")]
        public float[] Score { get; set; }
    }

    public class PriorityInput
    {
        // --- FIX IS HERE ---
        [ColumnName("Message"), LoadColumn(0)]
        public string RawMessage { get; set; }

        [ColumnName("Priority"), LoadColumn(1)]
        public string Priority { get; set; }
    }

    public class PriorityOutput
    {
        [ColumnName("PredictedLabel")]
        public string PredictedPriority { get; set; }

        [ColumnName("Score")]
        public float[] Score { get; set; }
    }
}