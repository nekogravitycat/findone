using System.Text.Json;
using StackExchange.Redis;

public class ConnectService
{
    private readonly IConnectionMultiplexer _redis;

    public ConnectService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<Connection> CreateConnection(string connectionId, string userId, string roomId){
        Connection connection = new(){
            ConnectionId = connectionId,
            UserId = userId,
            RoomId = roomId
        };
        await _redis.GetDatabase().StringSetAsync(connectionId, JsonSerializer.Serialize(connection));
        return connection;
    }

    public async Task<Connection?> GetUserIdFromConnectionId(string connectionId){
        RedisValue connection = await _redis.GetDatabase().StringGetAsync(connectionId);
        if (connection.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Connection>(connection!) ?? null;
    }

    public async Task<Connection?> GetRoomIdFromConnectionId(string connectionId){
        RedisValue connection = await _redis.GetDatabase().StringGetAsync(connectionId);
        if (connection.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Connection>(connection!) ?? null;
    }

    public async Task DeleteConnection(string connectionId){
        await _redis.GetDatabase().KeyDeleteAsync(connectionId);
    }
}
