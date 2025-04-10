using server.Models;
using server.Utils;
using StackExchange.Redis;
using System.Text.Json;

public class RoomService
{
    private readonly IConnectionMultiplexer _redis;

    public RoomService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<Room?> GetRoom(string roomId)
    {
        var db = _redis.GetDatabase(); 
        var json = await db.StringGetAsync($"room:{roomId}");
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<Room>(json!);
    }

    public async Task<Room> CreateRoom(User hostUser, int round, int timeLimit)
    {
        string roomId = IdGenerator.GenerateRoomId();
        string[] targets = { "Apple", "Banana", "Cherry", "Date", "Elderberry" };

        var room = new Room
        {
            RoomId = roomId,
            HostUserId = hostUser.UserId,
            Round = round,
            TimeLimit = timeLimit,
            Targets = targets.Select((t, i) => new RoomTarget
            {
                RoomId = roomId,
                RoundIndex = i + 1,
                TargetName = t,
                Room = null!
            }).ToList(),
            UserIds = new HashSet<Guid> { hostUser.UserId },
            Status = RoomStatus.Waiting
        };

        var db = _redis.GetDatabase();
        await db.StringSetAsync($"room:{roomId}", JsonSerializer.Serialize(room), TimeSpan.FromHours(2));
        return room;
    }

    public async Task<Room> JoinRoom(string roomId, User user)
    {
        var db = _redis.GetDatabase();

        var json = await db.StringGetAsync($"room:{roomId}");
        var room = JsonSerializer.Deserialize<Room>(json!) ?? throw new Exception("Room not found");
        room.UserIds.Add(user.UserId);
        await db.StringSetAsync($"room:{roomId}", JsonSerializer.Serialize(room), TimeSpan.FromHours(2));
        return room;
    }

    public async Task<Room> StartGame(string roomId)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync($"room:{roomId}");
        var room = JsonSerializer.Deserialize<Room>(json!) ?? throw new Exception("Room not found");
        room.Status = RoomStatus.InProgress;
        await db.StringSetAsync($"room:{roomId}", JsonSerializer.Serialize(room), TimeSpan.FromHours(2));
        return room;
    }
}
