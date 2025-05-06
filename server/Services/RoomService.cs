using server.Models;
using server.Utils;
using StackExchange.Redis;
using System.IO;
using System.Text.Json;

public class RoomService
{
    private readonly IConnectionMultiplexer _redis;

    public RoomService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<Room> CreateRoom(User hostUser, int round, int timeLimit)
    {
        Random random = new Random();
        string roomId = IdGenerator.GenerateRoomId();
        List<string> allTargets = File.ReadAllLines("Data/targets.txt")
                             .Where(line => !string.IsNullOrWhiteSpace(line))
                             .ToList() ?? throw new Exception("找不到檔案! 請檢查錯誤");

        List<string> targets = allTargets.OrderBy(_ => random.Next()).Take(round).ToList();

        Room room = new Room
        {
            RoomId = roomId,
            HostUserId = hostUser.UserId,
            Round = round,
            TimeLimit = timeLimit,
            Targets = targets.Select((t, i) => new RoomTarget
            {
                RoomId = roomId,
                TargetName = t,
            }).ToList(),
            UserIds = new HashSet<Guid> { hostUser.UserId },
            UserConnections = new HashSet<string> { hostUser.ConnectionId },
            Status = RoomStatus.Waiting
        };

        await UpdateRoom(room);
        return room;
    }

    public async Task<Room> JoinRoom(string roomId, User user)
    {
        IDatabase db = _redis.GetDatabase();

        var json = await db.StringGetAsync($"room:{roomId}");
        var room = JsonSerializer.Deserialize<Room>(json!) ?? throw new Exception("Room not found");

        // ensure room is not in progress or finished
        if (room.Status == RoomStatus.InProgress)
            throw new Exception("Room is already in progress");

        if (room.Status == RoomStatus.Finished)
            throw new Exception("Room is already finished");

        room.UserIds.Add(user.UserId);
        room.UserConnections.Add(user.ConnectionId);
        
        await UpdateRoom(room);

        return room;
    }

    public async Task<Room> StartGame(string roomId)
    {
        Room room = await GetRoom(roomId);

        room.Status = RoomStatus.InProgress;
        await UpdateRoom(room);

        return room;
    }

    public async Task<Room> AddSubmit(string userId, string roomId, DateTime datetime, int roundIndex)
    {
        Room room = await GetRoom(roomId);

        room.RoomSubmits[roundIndex].Add(new RoomSubmit
        {
            DateTime = datetime,
            UserId = Guid.Parse(userId)
        });

        await UpdateRoom(room);
        return room;
    }
    public async Task<Room> GetRoom(string roomId)
    {
        IDatabase db = _redis.GetDatabase();
        RedisValue json = await db.StringGetAsync($"room:{roomId}");

        if (string.IsNullOrEmpty(json))
            throw new Exception($"Room not found: {roomId}");

        return JsonSerializer.Deserialize<Room>(json!)!;
    }
    public async Task<Room?> UpdateRoom(Room room)
    {
        IDatabase db = _redis.GetDatabase();
        await db.StringSetAsync($"room:{room.RoomId}", JsonSerializer.Serialize(room), TimeSpan.FromHours(2));
        return room;
    }
}
