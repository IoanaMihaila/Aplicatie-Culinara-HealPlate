using System.Text.Json;
using System.Text;

namespace Aplicatie_Culinara_HealPlate.Services
{
    public class VisionAPIService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public VisionAPIService(string apiKey)
        {
            _apiKey = "AIzaSyAXJovmBYRkNuhFjHWxS6dc2LPv27eWF4s";
            _httpClient = new HttpClient();
        }

        public async Task<List<string>> DetectLabelsAsync(byte[] imageBytes, int maxResults = 5)
        {
            string base64Image = Convert.ToBase64String(imageBytes);
            var request = new
            {
                requests = new[]
                {
                new
                {
                    image = new { content = base64Image },
                    features = new[] { new { type = "LABEL_DETECTION", maxResults = maxResults } }
                }
            }
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"https://vision.googleapis.com/v1/images:annotate?key={_apiKey}", content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var labels = new List<string>();

            foreach (var label in doc.RootElement.GetProperty("responses")[0].GetProperty("labelAnnotations").EnumerateArray())
            {
                if (label.GetProperty("score").GetDouble() >= 0.7)
                    labels.Add(label.GetProperty("description").GetString().ToLower());
            }

            return labels;
        }
    }
}
