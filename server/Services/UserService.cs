using server.Models;
using System.Text.Json;
using StackExchange.Redis;

public class UserService
{
    private readonly IConnectionMultiplexer _redis;

    public UserService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    public async Task<User> CreateUser(string userName, string connectionId)
    {
        User user = new User { UserName = userName, ConnectionId = connectionId };
        await UpdateUser(user);
        return user;
    }

    public async Task<User> CreateUser(string userName, string roomId, string connectionId)
    {
        User user = new User { 
            UserName = userName, 
            RoomId = roomId,
            ConnectionId = connectionId
        };
        await UpdateUser(user);
        return user;
    }

    public async Task<bool> DeleteUser(string userId)
    {
        IDatabase db = _redis.GetDatabase();
        return await db.KeyDeleteAsync($"user:{userId}");
    }

    public async Task<User> AddScore(UserScore userScore)
    {
        IDatabase db = _redis.GetDatabase();
        RedisValue json = await db.StringGetAsync($"user:{userScore.UserId}");
        User user = (string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<User>(json!)) ?? throw new Exception("User not found");
        
        user.Scores.Add(userScore);

        user = await UpdateUser(user);
        return user;
    }

    public async Task<User> GetUser(string userId)
    {
        IDatabase db = _redis.GetDatabase();
        RedisValue json = await db.StringGetAsync($"user:{userId}");

        if(string.IsNullOrEmpty(json))
        {
            throw new Exception($"User not found: {userId}");
        }

        return JsonSerializer.Deserialize<User>(json!)!;
    }

    public async Task<User[]> GetUsers(List<Guid> userIds)
    {
        IDatabase db = _redis.GetDatabase();
        var tasks = userIds.Select(userId => db.StringGetAsync($"user:{userId}"));
        var results = await Task.WhenAll(tasks);
        return results
            .Where(json => !string.IsNullOrEmpty(json))
            .Select(json => JsonSerializer.Deserialize<User>(json!)!)
            .ToArray();
    }

    public async Task<User> UpdateUser(User user)
    {
        IDatabase db = _redis.GetDatabase();
        await db.StringSetAsync($"user:{user.UserId}", JsonSerializer.Serialize(user), TimeSpan.FromHours(2));
        return user;
    }
}
