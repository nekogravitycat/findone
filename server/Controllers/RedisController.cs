using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        // set a key-value pair
        [HttpPost("set")]
        public async Task<IActionResult> SetKey(string key, string value)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, value);
            return Ok($"Key: {key} set to {value}");
        }

        // get a value by key
        [HttpGet("get")]
        public async Task<IActionResult> GetKey(string key)
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            return Ok($"Key: {key} value: {value}");
        }

        // ex. increment a key
        [HttpPost("increment")]
        public async Task<IActionResult> IncrementKey(string key, int increment = 1)
        {
            var db = _redis.GetDatabase();
            var newValue = await db.StringIncrementAsync(key, increment);
            return Ok($"Key: {key} incremented to {newValue}");
        }

        // ex. delete a key
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteKey(string key)
        {
            var db = _redis.GetDatabase();
            bool deleted = await db.KeyDeleteAsync(key);
            return Ok($"Key: {key} deletion status: {deleted}");
        }
    }
}
