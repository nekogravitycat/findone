using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ���U Redis �s�u (�Юھڹ�� Redis �s�u�r��ק�)
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

    // ���浪�� API �d�ҡA���]�z�L AI ���ѵ��ץ��T��[��
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitAnswer([FromBody] AnswerRequest request)
    {
        // �o�����өI�s AI �ҫ��i��Ϥ����ѡA�P�_���ץ��T��
        bool isCorrect = true; // ���]���ץ��T

        if (isCorrect)
        {
            var db = _redis.GetDatabase();
            // �ھڪ��aID��s���ơA�Ҧp�C������[ 10 ��
            await db.StringIncrementAsync($"score:{request.PlayerId}", 10);
            return Ok(new { message = "���ץ��T�A���Ƥw��s" });
        }
        else
        {
            return BadRequest(new { message = "���׿��~" });
        }
    }
}

public class AnswerRequest
{
    public string PlayerId { get; set; }
    public string Answer { get; set; }
}
