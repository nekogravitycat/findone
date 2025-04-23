using StackExchange.Redis;

namespace server.Extensions
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration config)
        {
            string? redisConnection = config.GetConnectionString("RedisConnection");

            if (string.IsNullOrWhiteSpace(redisConnection))
            {
                throw new ArgumentNullException("RedisConnection", "Redis connection string is not set in appsettings.json");
            }

            try
            {
                var multiplexer = ConnectionMultiplexer.Connect(redisConnection);
                if (!multiplexer.IsConnected)
                {
                    throw new RedisConnectionException(ConnectionFailureType.UnableToResolvePhysicalConnection, "Connected but Redis is unusable.");
                }

                services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine($"❌ Redis 連線失敗：{ex.Message}");
                throw;
            }

            return services;
        }
    }
}
