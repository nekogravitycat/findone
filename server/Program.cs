using StackExchange.Redis;
using server.Hubs;
using server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container and add Redis Connection
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

// Add services to the container. (SignalR)
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024 * 5; // 5MB
});

builder.Services.AddControllers();

builder.Services.AddSingleton<ImageService>();
builder.Services.AddSingleton<GoogleAIService>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<UserService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(static endpoints =>
{
    _ = endpoints.MapControllers();
    _ = endpoints.MapHub<GameHub>("/gamehub");
});

app.Run();