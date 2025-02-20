using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 註冊 Redis 連線 (請根據實際 Redis 連線字串修改)
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost")
);

builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;

    public GameController(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    // 提交答案 API 範例，假設透過 AI 辨識答案正確後加分
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitAnswer([FromBody] AnswerRequest request)
    {
        // 這邊應該呼叫 AI 模型進行圖片辨識，判斷答案正確性
        bool isCorrect = true; // 假設答案正確

        if (isCorrect)
        {
            var db = _redis.GetDatabase();
            // 根據玩家ID更新分數，例如每次答對加 10 分
            await db.StringIncrementAsync($"score:{request.PlayerId}", 10);
            return Ok(new { message = "答案正確，分數已更新" });
        }
        else
        {
            return BadRequest(new { message = "答案錯誤" });
        }
    }
}

public class AnswerRequest
{
    public string PlayerId { get; set; }
    public string Answer { get; set; }
}
