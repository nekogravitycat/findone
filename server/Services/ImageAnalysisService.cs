using server.Models;
using server.Utils;

namespace server.Services
{
    public class ImageAnalysisService
    {
        private readonly GoogleAIService _googleAIService;
        private readonly string[] _imageObjects = { "glasses", "ball", "book", "elephant", "bottle" };
        
        public ImageAnalysisService(IConfiguration configuration)
        {
            _googleAIService = new GoogleAIService(configuration);
        }

        public async Task<ImageResponse?> AnalyzeImage(string base64Image)
        {
            string fixedImage = ImageHelper.FixBase64String(base64Image);
            string mimeType = ImageHelper.DetectMimeType(fixedImage);

            if (string.IsNullOrEmpty(mimeType))
                throw new FormatException("無法辨識的影像格式");

            var clientRequest = $"Analyze the given image and identify all objects within it. Focus specifically on the following objects: {string.Join(", ", _imageObjects)}\r\n";
            return await _googleAIService.AnalyzeImage(fixedImage, mimeType, clientRequest);
        }
    }
}
