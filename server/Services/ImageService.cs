using server.Models;
using server.Utils;


namespace server.Services
{
    public class ImageService
    {
        private readonly UserService _userService;
        private readonly GoogleAIService _googleAIService;
        private readonly RoomService _roomService;
        private readonly ScoreService _scoreService;
        
        public ImageService(IConfiguration configuration, UserService userService, RoomService roomService, ScoreService scoreService)
        {
            _googleAIService = new GoogleAIService(configuration);
            _userService = userService;
            _roomService = roomService;
            _scoreService = scoreService;
        }

        public async Task SubmitImage(string roomId, string userId, string base64Image, DateTime submitTime)
        {
            // validate room
            Room room = await _roomService.GetRoom(roomId);
            int round = room.CurrentRound ?? throw new Exception("Round not started");

            // validate user
            User user = await _userService.GetUser(userId);
            string target = room.Targets[round].TargetName;

            if (room.Status != RoomStatus.InProgress)
                throw new Exception("Game not started yet or already ended!");

            if (room.EndTime < submitTime)
                throw new Exception("Round already closed!");

            if (room.CurrentRound == null)
                throw new Exception("Round not started yet!");

            // check if image already submitted
            bool alreadySubmitted = user.Scores.Any(s => s.RoundIndex == round);
            if (alreadySubmitted) throw new Exception("Image already submitted for this round!");

            // analyze
            ImageResponse result = await AnalyzeImage(base64Image, target) 
                ?? throw new Exception("Image analysis failed");

            if (!result.Match) throw new Exception("Image does not match the target");

            UserScore score = new UserScore
            {
                UserId = Guid.Parse(userId),
                RoundIndex = round,
                Comment = result.Comment,
                DateTime = submitTime,
                Base64Image = base64Image,
                Score = _scoreService.CalculateScore(room, submitTime)
            };

            // add score record to user
            await _userService.AddScore(score);
            // add submit record to room
            await _roomService.AddSubmit(userId, roomId, submitTime, round);
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
