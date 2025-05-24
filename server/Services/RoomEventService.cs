using Microsoft.AspNetCore.SignalR;
using server.Hubs;
using server.Models;
using StackExchange.Redis;

namespace server.Services
{
    public class RoomEventService : BackgroundService
    {
        private readonly IDatabase _db;
        private readonly ImageService _imageService;
        private readonly RoomService _roomService;
        private readonly RankService _rankService;
        private readonly IHubContext<GameHub> _hubContext;
        private const string StreamKey = "room:submit:stream";

        public RoomEventService(ImageService imageService, RoomService roomService, RankService rankService, IConnectionMultiplexer redis, IHubContext<GameHub> hubContext)
        {
            _db = redis.GetDatabase();
            _imageService = imageService;
            _roomService = roomService;
            _rankService = rankService;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create the stream if it doesn't exist
            try
            {
                await _db.StreamCreateConsumerGroupAsync(StreamKey, "room:submit:group", "0-0", true);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
            {
                // Ignore if the group already exists
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                // read new messages
                StreamEntry[] entries = await _db.StreamReadGroupAsync(
                    StreamKey, "room:submit:group", "consumer-1", ">", 1);

                if (entries.Length == 0)
                {
                    // No new messages are available in the stream. Check for pending messages that were not acknowledged
                    // by any consumer. This ensures that unprocessed messages are not left in the stream indefinitely.
                    var pending = await _db.StreamPendingMessagesAsync(StreamKey, "room:submit:group", 10, "consumer-1");

                    foreach (var pendingMessage in pending)
                    {
                        var claimedEntries = await _db.StreamClaimAsync(StreamKey, "room:submit:group", "consumer-1", 0, new[] { pendingMessage.MessageId });

                        foreach (var entry in claimedEntries)
                        {
                            await ProcessEntry(entry);
                        }
                    }
                }
                else
                {
                    foreach (var entry in entries)
                        await ProcessEntry(entry);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessEntry(StreamEntry entry)
        {
            try
            {
                string userId = entry["UserId"].ToString();
                string roomId = entry["RoomId"].ToString();
                string base64Image = entry["Base64Image"].ToString();
                string connectionId = entry["connectionId"].ToString();
                DateTime submitTime = DateTime.Parse(entry["SubmitTime"].ToString());

                // submit the image
                await _imageService.SubmitImage(roomId, userId, base64Image, submitTime);

                await _hubContext.Clients.Client(connectionId).SendAsync($"ImageAnalysisSucceeded");

                Room room = await _roomService.GetRoom(roomId);

                // check if all users have submitted
                int currentRound = room.CurrentRound ?? throw new Exception("Round not started yet!");
                List<RoomSubmit> currentRoundSubmits = room.RoomSubmits[currentRound];
                HashSet<Guid> submittedUserIds = currentRoundSubmits.Select(s => s.UserId).ToHashSet();

                if (submittedUserIds.Count == room.UserIds.Count && currentRoundSubmits.Count == room.UserIds.Count)
                {
                    List<Score> scores = await _rankService.GetRankInfo(roomId, userId);
                    await _hubContext.Clients.Group(roomId).SendAsync("RankInfo", scores);
                }

                // acknowledge the entry
                await _db.StreamAcknowledgeAsync(StreamKey, "room:submit:group", entry.Id);
            }
            catch (Exception ex)
            {
                string connectionId = entry["connectionId"].ToString();
                await _hubContext.Clients.Client(connectionId).SendAsync("ImageAnalysisFailed", ex.Message);

                await _db.StreamAcknowledgeAsync(StreamKey, "room:submit:group", entry.Id);
            }
        }

        public async Task EnqueueSubmitAsync(string connectionId, string roomId, string userId, string base64Image)
        {
            await _db.StreamAddAsync(StreamKey, [
                new("UserId", userId),
                new("RoomId", roomId),
                new("Base64Image", base64Image),
                new("connectionId", connectionId),
                new("SubmitTime", DateTime.UtcNow.ToString()),
            ]);
        }
    }
}
