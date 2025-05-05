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
        private readonly IHubContext<GameHub> _hubContext;
        private const string StreamKey = "room:submit:stream";

        public RoomEventService(ImageService imageService, UserService userService, IConnectionMultiplexer redis, IHubContext<GameHub> hubContext)
        {
            _db = redis.GetDatabase();
            _imageService = imageService;
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

            // Start processing the stream
            while (!stoppingToken.IsCancellationRequested)
            {
                StreamEntry[] entries = await _db.StreamReadGroupAsync(
                    StreamKey, "room:submit:group", "consumer-1", ">", 1);

                foreach (StreamEntry entry in entries)
                {
                    string? connectionId = null;

                    try
                    {
                        string userId = entry["UserId"].ToString();
                        string roomId = entry["RoomId"].ToString();
                        string base64Image = entry["Base64Image"].ToString();
                        connectionId = entry["connectionId"].ToString();
                        DateTime submitTime = DateTime.Parse(entry["SubmitTime"].ToString());

                        // Process the image submission
                        await _imageService.SubmitImage(roomId, userId, base64Image, submitTime);

                        await _hubContext.Clients.Client(connectionId).SendAsync("ImageAnalysisSucceeded");
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception (e.g., log it)
                        Console.WriteLine($"Error processing stream entry: {ex.Message}");
                        if(connectionId != null)
                            await _hubContext.Clients.Client(connectionId).SendAsync("ImageAnalysisFailed", ex.Message);
                    }
                    finally
                    {
                        // Acknowledge the entry after processing
                        await _db.StreamAcknowledgeAsync(StreamKey, "room:submit:group", entry.Id);
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        public async Task EnqueueSubmitAsync(string connectionId, string roomId, string userId, string base64Image)
        {
            await _db.StreamAddAsync(StreamKey, new NameValueEntry[]
            {
                new("UserId", userId),
                new("RoomId", roomId),
                new("Base64Image", base64Image),
                new("connectionId", connectionId),
                new("SubmitTime", DateTime.UtcNow.ToString()),
            });
        }
    }
}
