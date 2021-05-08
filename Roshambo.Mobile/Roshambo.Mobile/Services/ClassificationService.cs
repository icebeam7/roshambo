using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

using Newtonsoft.Json;

using Roshambo.Models;
using System.IO;
using Xamarin.Essentials;

namespace Roshambo.Mobile.Services
{
    public class ClassificationService
    {
        private const string RoshamboMLEndpoint = "https://roshambo-webapi.azurewebsites.net/";
        private const string classifyApi = "api/Classify";

        private static readonly HttpClient client = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(RoshamboMLEndpoint);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }

        public async static Task<HandGesturePrediction> Classify(FileResult file)
        {
            using (var image = await file.OpenReadAsync())
            {
                HttpContent fileStreamContent = new StreamContent(image);

                var fn = $"{Guid.NewGuid()}.jpeg";

                fileStreamContent.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fn };

                fileStreamContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(fileStreamContent);
                    var response = await client.PostAsync(classifyApi, formData);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var mlGesture = JsonConvert.DeserializeObject<HandGesturePrediction>(json);
                        return mlGesture;
                    }
                }
            }

            return new HandGesturePrediction();
        }
    }
}
