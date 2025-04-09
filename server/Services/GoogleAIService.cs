using Mscc.GenerativeAI;
using server.Models;
using System.Text.Json;

namespace server.Services
{
    public class GoogleAIService
    {
        private readonly string _apiKey;
        private readonly string _systemInstruction = "You are a professional image analysis assistant. " +
            "Analyze the given image and identify all the objects within it. " +
            "Please list every detected object along with its confidence score in descending order of confidence. " +
            "The confidence score should range between 0.00 and 1.00, rounded to two decimal places. " +
            "Include all objects even if their confidence is lower, as long as they are detected.\r\n";

        private readonly string _responseSchema = @"
        {
            ""type"": ""object"",
            ""properties"": {
                ""Object"": { 
                ""type"": ""array"", 
                ""items"": { ""type"": ""string"" } 
            },
            ""Confidence"": { 
                ""type"": ""array"", 
                ""items"": { ""type"": ""string"" } }
            },
            ""required"": [""Object"", ""Confidence""]
        }";

        public GoogleAIService(IConfiguration configuration)
        {
            _apiKey = configuration["APIKeys:GOOGLE_API_KEY"];
        }

        public async Task<ImageResponse?> AnalyzeImage(string base64Image, string mimeType, string clientRequest)
        {
            var googleAI = new GoogleAI(_apiKey);
            var systemInstruction = new Content(_systemInstruction);
            var model = googleAI.GenerativeModel(Model.Gemini20Flash, systemInstruction: systemInstruction);
            var generationConfig = new GenerateContentConfig
            {
                ResponseMimeType = "application/json",
                ResponseSchema = _responseSchema
            };
            var query = new GenerateContentRequest(clientRequest, generationConfig);
            await query.AddMedia(base64Image, mimeType: mimeType);

            var response = await model.GenerateContent(query);
            Console.WriteLine($"Response: {response.Text}");
            return string.IsNullOrEmpty(response.Text) ? null : JsonSerializer.Deserialize<ImageResponse>(response.Text);
        }
    }
}
