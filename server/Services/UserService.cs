using server.Models;
using System.Text.Json;
using StackExchange.Redis;
using Microsoft.AspNetCore.Identity;

public class UserService
{
    private readonly IConnectionMultiplexer _redis;

    public UserService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    public async Task<User> CreateUser(string userName)
    {
        var user = new User { UserName = userName };
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"user:{user.UserId}", JsonSerializer.Serialize(user), TimeSpan.FromHours(2));
        return user;
    }
    public async Task<User> CreateUser(string userName, string roomId)
    {
        var user = new User { UserName = userName, RoomId = roomId};
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"user:{user.UserId}", JsonSerializer.Serialize(user), TimeSpan.FromHours(2));
        return user;
    }

    public async Task<User> UpdateUser(User user)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"user:{user.UserId}", JsonSerializer.Serialize(user), TimeSpan.FromHours(2));
        return user;
    }

    public async Task<User> AddScore(UserScore userScore)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync($"user:{userScore.UserId}");
        User user = (string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<User>(json!)) ?? throw new Exception("User not found");
        
        user.Scores.Add(userScore);

        user = await UpdateUser(user);
        return user;
    }

    public async Task<User?> GetUser(string userId)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync($"user:{userId}");
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<User>(json!);
    }
}
