using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.ML;
using Microsoft.ML.Vision;

using Roshambo.Models;

namespace Roshambo.ML
{
    class Program
    {
        private static string basePath = "/Users/luisbeltran/Projects/VirtualMLNet";

        private static string dataPath = Path.Combine(basePath, "Data");
        private static string trainingImagesPath = Path.Combine(dataPath, "rps");
        private static string validationImagesPath = Path.Combine(dataPath, "rps-validation");
        private static string testImagesPath = Path.Combine(dataPath, "rps-test");

        private static string modelPath = Path.Combine(basePath, "Model");
        private static string mlModelPath = Path.Combine(modelPath, "RoshamboModel.zip");
        private static string logPath = Path.Combine(modelPath, "log.txt");

        private static string[] labels = new string[] { "rock", "paper", "scissors" };

        private static IEnumerable<HandGesture> PrepareSet(string folderPath, string label = "", string subset = "")
        {
            string[] formats = { ".png", ".jpg", ".jpeg" };

            foreach (var file in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories).Where(x => formats.Any(x.EndsWith)))
            {
                yield return new HandGesture()
                {
                    ImagePath = Path.GetFullPath(file),
                    ActualGesture = label,
                    Subset = subset
                };
            }
        }

        private static IEnumerable<HandGesture> LoadImages(string folderPath, string subset)
        {
            var dataset = new List<HandGesture>();

            foreach (var label in labels)
            {
                var labelFolder = Path.Combine(folderPath, label);
                dataset.AddRange(PrepareSet(labelFolder, label, subset));
            }

            return dataset;
        }

        private static void PrintMessage(string message)
        {
            var messageTime = $"{DateTime.Now:hh.mm.ss.ffffff} -> {message}...";
            Console.WriteLine(messageTime);

            using (var sw = File.AppendText(logPath))
                sw.WriteLine(messageTime);
        }

        static void Main(string[] args)
        {
            /*
            var context = new MLContext();

            PrintMessage("Loading data");
            var trainingImages = LoadImages(trainingImagesPath, "T");
            var validationImages = LoadImages(validationImagesPath, "V");

            PrintMessage($"Training: {trainingImages.Count()} images");
            PrintMessage($"Validation: {validationImages.Count()} images");

            var trainingImagesDataView = context.Data.LoadFromEnumerable(trainingImages);
            var validationImagesDataView = context.Data.LoadFromEnumerable(validationImages);

            var loadPipeline = context.Transforms.LoadRawImageBytes(
                outputColumnName: "ImageBytes",
                imageFolder: null,
                inputColumnName: "ImagePath");

            var trainingOptions = new ImageClassificationTrainer.Options
            {
                FeatureColumnName = "ImageBytes",
                LabelColumnName = "EncodedLabel",
                WorkspacePath = "workspace",
                Arch = ImageClassificationTrainer.Architecture.InceptionV3,
                ReuseTrainSetBottleneckCachedValues = true,
                MetricsCallback = (metrics) => Console.WriteLine(metrics.ToString())
            };

            var trainingPipeline = context.Transforms
                .Conversion.MapValueToKey(outputColumnName: "EncodedLabel",
                                        inputColumnName: "ActualGesture")
                    .Append(context.MulticlassClassification.Trainers.ImageClassification(trainingOptions))
                    .Append(context.Transforms.Conversion
                        .MapKeyToValue(outputColumnName: "PredictedLabel",
                                        inputColumnName: "PredictedLabel"));

            var fullPipeline = loadPipeline.Append(trainingPipeline);
            PrintMessage("Training starts.");

            var model = fullPipeline.Fit(trainingImagesDataView);

            PrintMessage("Training completed.");

            PrintMessage("Validating model.");
            var predictionsDataView = model.Transform(validationImagesDataView);

            Console.WriteLine("Metrics:");
            var evaluation = context.MulticlassClassification.Evaluate(
                predictionsDataView,
                labelColumnName: "EncodedLabel");

            Console.WriteLine($"  * Macro Accuracy: {evaluation.MacroAccuracy}");
            PrintMessage("Validation completed.");

            PrintMessage("Image classification #1.");
            var predictions = context.Data.CreateEnumerable<HandGesturePrediction>(
                predictionsDataView,
                reuseRowObject: true);

            foreach (var item in predictions)
            {
                var image = Path.GetFileName(item.ImagePath);
                PrintMessage($"* Image: {image} | Actual gesture: {item.ActualGesture} | Predicted: {item.PredictedLabel}");
            }

            PrintMessage("Saving model");
            context.Model.Save(model, trainingImagesDataView.Schema, mlModelPath);

            PrintMessage("Model saved");
            */
            PrintMessage("Classifying several images");
            ConsumingModel(testImagesPath);
        }

        private static void ConsumingModel(string imagesPath)
        {
            DataViewSchema dataViewSchema;
            var context = new MLContext();

            PrintMessage("Loading model");
            var model = context.Model.Load(mlModelPath, out dataViewSchema);

            var data = PrepareSet(imagesPath);

            var imagesDataView = context.Data.LoadFromEnumerable(data);
            PrintMessage("Model loaded");

            ClassifyImages(context, imagesDataView, model);
        }

        private static void ClassifyImages(MLContext context, IDataView data, ITransformer model)
        {
            var predictionEngine = context.Model
                .CreatePredictionEngine<HandGesture, HandGesturePrediction>(model);

            var images = context.Data.CreateEnumerable<HandGesture>(data, reuseRowObject: true);
            PrintMessage("Classifying images");

            foreach (var item in images)
            {
                var prediction = predictionEngine.Predict(item);

                var image = Path.GetFileName(prediction.ImagePath);
                PrintMessage($"* Image: {image} | Predicted gesture: {prediction.PredictedLabel}");
            }
        }
    }
}
