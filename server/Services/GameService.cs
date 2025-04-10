using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace server.Services
{

    public class GameService
    {
        private readonly RoomService _roomService;
        private readonly UserService _userService;

        public GameService(RoomService roomService, UserService userService)
        {
            _roomService = roomService;
            _userService = userService;
        }

        public async Task HandleGetUser(HubCallerContext context, IClientProxy caller, string userId)
        {
            var user = await _userService.GetUser(userId);
            if (user == null)
            {
                await caller.SendAsync("UserNotFound", userId);
                return;
            }
            await caller.SendAsync("UserFound", user);
        }

        public async Task HandleCreateRoom(HubCallerContext context, IClientProxy caller, IGroupManager groups, string userName, int round, int timeLimit)
        {
            var user = await _userService.CreateUser(userName);
            var room = await _roomService.CreateRoom(user, round, timeLimit);
            await groups.AddToGroupAsync(context.ConnectionId, room.RoomId);
            await caller.SendAsync("RoomCreated", room.RoomId);
        }

        public async Task HandleJoinRoom(HubCallerContext context, IClientProxy caller, IGroupManager groups, string roomId, string userName)
        {
            var user = await _userService.CreateUser(userName);
            var room = await _roomService.JoinRoom(roomId, user);

            // update roomId
            user.RoomId = room.RoomId;
            await _userService.UpdateUser(user);

            await groups.AddToGroupAsync(context.ConnectionId, roomId);
            await caller.SendAsync("GameJoined", roomId, user);
        }

        public async Task HandleStartGame(IHubCallerClients clients, string roomId)
        {
            var room = await _roomService.StartGame(roomId);
            await clients.Group(roomId).SendAsync("GameStarted", roomId, room);
        }

        public async Task HandleCheckAnswer(IHubCallerClients clients, string roomId, string userId, int roundIndex, string base64Image)
        {
            // todo: image analysis logic
            await clients.Caller.SendAsync("AnswerChecked", roomId, userId, roundIndex, "Pending");
        }
    }
}