using server.Models;
using server.Utils;

namespace server.Services
{
    public class ImageService
    {
        private readonly GoogleAIService _googleAIService;
        private readonly RoomService _roomService;
        
        public ImageService(IConfiguration configuration, RoomService roomService)
        {
            _googleAIService = new GoogleAIService(configuration);
            _roomService = roomService;
        }

        public async Task<ImageResponse?> AnalyzeImage(string base64Image, string target)
        {
            string fixedImage = ImageHelper.FixBase64String(base64Image);
            string mimeType = ImageHelper.DetectMimeType(fixedImage);

            if (string.IsNullOrEmpty(mimeType))
                throw new FormatException("無法辨識的影像格式");

            var clientRequest = $"Prompt: \"{target}\"\r\n" +
                $"Please analyze the image and determine if it matches the prompt.\r\n";
            return await _googleAIService.AnalyzeImage(base64Image, mimeType, clientRequest);
        }
    }
}
