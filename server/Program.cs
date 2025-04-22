using server.Extensions;
using server.Hubs;
using server.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Redis
builder.Services.AddRedis(builder.Configuration);

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024 * 5; // 5MB
});

// Register services
builder.Services.AddSingleton<ImageService>();
builder.Services.AddSingleton<GoogleAIService>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ScoreService>();

// Add Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(static endpoints =>
{
    _ = endpoints.MapControllers();
    _ = endpoints.MapHub<GameHub>("/gamehub");
});

app.Run();