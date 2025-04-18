using Microsoft.AspNetCore.SignalR;
using server.Services;
using StackExchange.Redis;

namespace server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly GameService _gameService;

        public GameHub(IConnectionMultiplexer redis, GameService gameService)
        {
            _redis = redis;
            _gameService = gameService;
        }

        public async Task GetUser(string userId)
            => await _gameService.HandleGetUser(Context, Clients.Caller, userId);

        public async Task GetRoom(string roomId)
            => await _gameService.HandleGetRoom(Context, Clients.Caller, roomId);

        public async Task CreateRoom(string userName, int round, int timeLimit)
            => await _gameService.HandleCreateRoom(Context, Clients.Caller, Groups, userName, round, timeLimit);

        public async Task GameJoin(string roomId, string userName)
            => await _gameService.HandleJoinRoom(Context, Clients.Caller, Groups, roomId, userName);

        public async Task GameStart(string roomId, string userId)
            => await _gameService.HandleStartGame(Clients, roomId, userId);

        public async Task SubmitImage(string userId, int roundIndex, string base64Image)
            => await _gameService.HandleSubmitImage(Clients.Caller, userId, roundIndex, base64Image);

        public async Task SendMessage(string user, string message)
            => await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
