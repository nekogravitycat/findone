using Microsoft.AspNetCore.SignalR;
using server.Services;

namespace server.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameService _gameService;

        public GameHub(GameService gameService)
        {
            _gameService = gameService;
        }

        public async Task GetUser(string userId)
            => await _gameService.HandleGetUser(Context, Clients.Caller, userId);

        public async Task GetRoom(string roomId)
            => await _gameService.HandleGetRoom(Context, Clients.Caller, roomId);

        public async Task CreateRoom(string userName, int round, int timeLimit)
            => await _gameService.HandleCreateRoom(Context, Clients.Caller, Groups, userName, round, timeLimit);

        public async Task GameJoin(string roomId, string userName)
            => await _gameService.HandleJoinRoom(Context, Clients, Clients.Caller, Groups, roomId, userName);

        public async Task GameStart(string roomId, string userId)
            => await _gameService.HandleStartGame(Clients, roomId, userId);

        public async Task GetRound(string roomId, string userId, int roundIndex)
            => await _gameService.HandleGetRound(Clients, roomId, userId, roundIndex);

        public async Task SubmitImage(string roomId, string userId, string base64Image)
            => await _gameService.HandleSubmitImage(Context.ConnectionId, roomId, userId, base64Image);

        public async Task GetRank(string roomId, string userId)
            => await _gameService.HandleGetRank(Clients, roomId, userId);

        public async Task SendMessage(string user, string message)
            => await Clients.All.SendAsync("ReceiveMessage", user, message);

        public override async Task OnDisconnectedAsync(Exception? exception)
            => await _gameService.HandleUserDisconnected(Clients, Context.ConnectionId);
    }
}
