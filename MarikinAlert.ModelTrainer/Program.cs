using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace MarikinAlert.ModelTrainer
{
    public class ModelInput
    {
        [LoadColumn(0)] public string Message { get; set; }
        [LoadColumn(1)] public string Category { get; set; }
        [LoadColumn(2)] public string Priority { get; set; }
    }

    public class ModelOutput
    {
        [ColumnName("PredictedLabel")] public string Prediction { get; set; }
        public float[] Score { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var mlContext = new MLContext(seed: 1); // Seed 1 ensures you get the same result every time

            // 1. LOCATE DATA
            string dataPath = Path.Combine(Environment.CurrentDirectory, "marikina_dataset.csv");
            if (!File.Exists(dataPath)) { Console.WriteLine("CSV not found!"); return; }

            Console.WriteLine($"Loading data from {dataPath}...");
            IDataView dataView = mlContext.Data.LoadFromTextFile<ModelInput>(path: dataPath, hasHeader: true, separatorChar: ',');
            
            // Split: 80% Training, 20% Testing
            var dataSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // =========================================================
            // TRAIN CATEGORY MODEL (Back to the Champion: OVA + SDCA)
            // =========================================================
            Console.WriteLine("\nTraining Category Model (OVA + SDCA)...");

            var categoryPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ModelInput.Category))
                .Append(mlContext.Transforms.Text.FeaturizeText("Features", nameof(ModelInput.Message)))
                // REVERTED TO SdcaLogisticRegression
                .Append(mlContext.MulticlassClassification.Trainers.OneVersusAll(
                    mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features")))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var categoryModel = categoryPipeline.Fit(dataSplit.TrainSet);

            // Evaluate
            var catMetrics = mlContext.MulticlassClassification.Evaluate(categoryModel.Transform(dataSplit.TestSet));
            Console.WriteLine($"Category Accuracy: {catMetrics.MacroAccuracy:P2}");

            // Save
            mlContext.Model.Save(categoryModel, dataView.Schema, "CategoryModel.zip");
            Console.WriteLine("Saved 'CategoryModel.zip'");

            // =========================================================
            // TRAIN PRIORITY MODEL (Back to the Champion: OVA + SDCA)
            // =========================================================
            Console.WriteLine("\nTraining Priority Model (OVA + SDCA)...");

            var priorityPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ModelInput.Priority))
                .Append(mlContext.Transforms.Text.FeaturizeText("Features", nameof(ModelInput.Message)))
                // REVERTED TO SdcaLogisticRegression
                .Append(mlContext.MulticlassClassification.Trainers.OneVersusAll(
                    mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features")))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var priorityModel = priorityPipeline.Fit(dataSplit.TrainSet);

            // Evaluate
            var prioMetrics = mlContext.MulticlassClassification.Evaluate(priorityModel.Transform(dataSplit.TestSet));
            Console.WriteLine($"Priority Accuracy: {prioMetrics.MacroAccuracy:P2}");

            // Save
            mlContext.Model.Save(priorityModel, dataView.Schema, "PriorityModel.zip");
            Console.WriteLine("Saved 'PriorityModel.zip'");

            Console.WriteLine("\nDONE! You can now copy the .zip files to your Web App.");
        }
    }
}