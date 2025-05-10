using server.Extensions;
using server.Hubs;
using server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add cors policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://findone.gravitycat.tw")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ScoreService>();
builder.Services.AddSingleton<ConnectService>();
builder.Services.AddSingleton<RankService>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<RoomEventService>();
builder.Services.AddHostedService<RoomEventService>();

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

app.UseCors();

app.UseAuthorization();

app.UseEndpoints(static endpoints =>
{
    _ = endpoints.MapControllers();
    _ = endpoints.MapHub<GameHub>("/gamehub");
});

app.Run();