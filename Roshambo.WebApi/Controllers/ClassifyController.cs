using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.ML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Roshambo.Models;
using Roshambo.WebApi.Helpers;
using Newtonsoft.Json;

namespace Roshambo.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassifyController : ControllerBase
    {
        private readonly PredictionEnginePool<HandGesture, HandGesturePrediction> _predictionEnginePool;

        public ClassifyController(PredictionEnginePool<HandGesture, HandGesturePrediction> predictionEnginePool)
        {
            _predictionEnginePool = predictionEnginePool;
        }

        [HttpPost]
        public async Task<ActionResult> Post(IFormFile file)
        {
            var gesture = new HandGesture();

            try
            {
                var prediction = new HandGesturePrediction();
                var localPath = await WriteFile(file);

                gesture = new HandGesture()
                {
                    ImagePath = localPath
                };

                if (!localPath.StartsWith("Error"))
                    //if (!string.IsNullOrWhiteSpace(localPath))
                    prediction = _predictionEnginePool.Predict(modelName: Constants.ModelName, example: gesture);
                else
                    prediction.PredictedLabel = localPath;
                    //prediction.PredictedLabel = "N/A";

                return Ok(prediction);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
                /*
                var s = JsonConvert.SerializeObject(ex);
                gesture.ActualGesture = s;
                return Ok(gesture);*/
            }
        }

        private async Task<string> WriteFile(IFormFile file)
        {
            var localFilePath = string.Empty;

            try
            {
                var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "files");

                if (!Directory.Exists(filesPath))
                    Directory.CreateDirectory(filesPath);

                var extension = Path.GetExtension(file.FileName);
                var localFileName = $"{Guid.NewGuid()}{extension}";
                localFilePath = Path.Combine(filesPath, localFileName);

                using (var stream = new FileStream(localFilePath, FileMode.Create))
                    await file.CopyToAsync(stream);
            }
            catch (Exception e)
            {
                var s = JsonConvert.SerializeObject(e);
                return $"Error {s}";
            }

            return localFilePath;
        }
    }
}