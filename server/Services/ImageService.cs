using server.Models;
using server.Utils;

namespace server.Services
{
    public class ImageService
    {
        private readonly GoogleAIService _googleAIService;
        private readonly RoomService _roomService;
        private readonly string[] _imageObjects = { "glasses", "ball", "book", "elephant", "bottle" };
        
        public ImageService(IConfiguration configuration, RoomService roomService)
        {
            _googleAIService = new GoogleAIService(configuration);
            _roomService = roomService;
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

        public async Task<float?> isImageCorrect(ImageResponse obj, string roomId, string userId, int roundIndex)
        {
            // already check room is validate
            Room? room = await _roomService.GetRoom(roomId);
            string target = room.Targets[roundIndex].TargetName;
            int i = 0;

            foreach (string objectName in obj.Object)
            {
                if (objectName.ToLower() == target.ToLower())
                    return float.Parse(obj.Confidence[i]);

                i++;
            }

            return null;
        }
    }
}
