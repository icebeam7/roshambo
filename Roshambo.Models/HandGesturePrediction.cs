namespace Roshambo.Models
{
    public class HandGesturePrediction
    {
        public string ImagePath { get; set; }

        public string ActualGesture { get; set; }

        public string PredictedLabel { get; set; }
    }
}