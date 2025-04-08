using Microsoft.AspNetCore.Mvc;
using Mscc.GenerativeAI;
using System.Text.Json;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ImageAnalysisController : Controller
    {
        private readonly string _apiKey;
        private readonly string _systemInstruction = "You are a professional image analysis assistant. " +
            "Analyze the given image and identify all the objects within it. " +
            "Please list every detected object along with its confidence score in descending order of confidence. " +
            "The confidence score should range between 0.00 and 1.00, rounded to two decimal places. " +
            "Include all objects even if their confidence is lower, as long as they are detected.\r\n";

        private readonly string _clientRequest = "Analyze the given image and identify all objects within it. " +
            "Focus specifically on the following objects: ";

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
                    ""items"": { ""type"": ""string"" }
                }
            },
            ""required"": [""Object"", ""Confidence""]
        }";

        public ImageAnalysisController(IConfiguration configuration)
        {
            _apiKey = configuration["APIKeys:GOOGLE_API_KEY"];
        }
        public class ImageRequest
        {
            public string Base64Image { get; set; }
        }
        public class ImageResponse
        {
            public string[] Object { get; set; }
            public string[] Confidence { get; set; }
        }

        [HttpPost("image")]
        public async Task<IActionResult> ImageAnalysis([FromBody] ImageRequest request)
        {
            try
            {

                if (request == null || string.IsNullOrEmpty(request.Base64Image))
                    return BadRequest(new { error = "請提供有效的 Base64 圖片" });


                string base64Image = FixBase64String(request.Base64Image);

                // detect MIME type
                string mimeType = DetectMimeType(base64Image);
                if (string.IsNullOrEmpty(mimeType))
                    return BadRequest(new { error = "無法辨識的影像格式" });

                // init gemini-20 flash model
                var googleAI = new GoogleAI(apiKey: _apiKey);
                var systemInstruction = new Content(_systemInstruction);
                var model = googleAI.GenerativeModel(model: Model.Gemini20Flash, systemInstruction: systemInstruction);

                // TODO: setup how to render imageObject and algorithm.
                string[] imageObjects = ["glasses", "ball", "book", "elephant", "bottle"];

                // set the request
                var clientRequest = _clientRequest + string.Join(", ", imageObjects) + "\r\n";

                var generationConfig = new GenerationConfig()
                {
                    ResponseMimeType = "application/json",
                    ResponseSchema = _responseSchema
                };
                var query = new GenerateContentRequest(clientRequest, generationConfig);
                await query.AddMedia(base64Image, mimeType: mimeType);

                var response = await model.GenerateContent(query);
                var responseText = response.Text;

                if (string.IsNullOrEmpty(responseText)) {
                    return BadRequest(new { error = "無法獲取回應，請重新嘗試!" });
                }

                var responseJson = JsonSerializer.Deserialize<ImageResponse>(responseText);

                if (responseJson == null)
                    return BadRequest(new { error = "無法解析的回應，請重新嘗試!" });

                return Ok(new { responseJson });
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "提供的 Base64 字串格式錯誤" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "影像分析失敗", details = ex.Message });
            }
        }

        private static string FixBase64String(string base64)
        {
            int mod4 = base64.Length % 4;
            if (mod4 > 0)
            {
                base64 += new string('=', 4 - mod4);
            }
            return base64;
        }
        private static string DetectMimeType(string base64)
        {
            if (base64.StartsWith("/9j/")) return "image/jpeg";
            if (base64.StartsWith("iVBORw0KGgo")) return "image/png";
            if (base64.StartsWith("R0lGODlh")) return "image/gif";
            if (base64.StartsWith("UklGR")) return "image/webp";
            return string.Empty;
        }
    }
}