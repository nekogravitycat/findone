using System.Runtime.CompilerServices;
using System.Text.Json;
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

        public async Task CreateRoom(string userName, int round, int timeLimit)
            => await _gameService.HandleCreateRoom(Context, Clients.Caller, Groups, userName, round, timeLimit);

        public async Task JoinRoom(string roomId, string userName)
            => await _gameService.HandleJoinRoom(Context, Clients.Caller, Groups, roomId, userName);

        public async Task StartGame(string roomId)
            => await _gameService.HandleStartGame(Clients, roomId);

        public async Task CheckAnswer(string roomId, string userId, int roundIndex, string base64Image)
            => await _gameService.HandleCheckAnswer(Clients, roomId, userId, roundIndex, base64Image);

        public async Task SendMessage(string user, string message)
            => await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
