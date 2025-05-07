using Humanizer;
using Microsoft.AspNetCore.SignalR;
using server.Models;
using static server.Models.Response;

namespace server.Services
{

    public class GameService
    {
        private readonly RoomService _roomService;
        private readonly RoomEventService _roomEventService;
        private readonly UserService _userService;
        private readonly ConnectService _connectService;
        private readonly RankService _rankService;

        public GameService(RoomService roomService, RoomEventService roomEventService, UserService userService, ConnectService connectService, RankService rankService)
        {
            _roomService = roomService;
            _roomEventService = roomEventService;
            _userService = userService;
            _connectService = connectService;
            _rankService = rankService;
        }

        public async Task HandleGetUser(HubCallerContext context, IClientProxy caller, string userId)
        {
            try
            {
                User user = await _userService.GetUser(userId) ?? throw new Exception("User not found");

                await caller.SendAsync($"UserFound:{userId}", user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await caller.SendAsync($"UserNotFound:{userId}", userId, ex.Message);
            }
        }

        public async Task HandleGetRoom(HubCallerContext context, IClientProxy caller, string roomId)
        {
            try
            {
                Room room = await _roomService.GetRoom(roomId) ?? throw new Exception("Room not found");
                await caller.SendAsync("RoomFound", room);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await caller.SendAsync("RoomNotFound", roomId, ex.Message);
            }
        }

        public async Task HandleCreateRoom(HubCallerContext context, IClientProxy caller, IGroupManager groups, string userName, int round, int timeLimit)
        {
            try
            {

                User user = await _userService.CreateUser(userName, context.ConnectionId);
                Room room = await _roomService.CreateRoom(user, round, timeLimit);
                await _connectService.CreateConnection(context.ConnectionId, user.UserId.ToString(), room.RoomId);

                user.RoomId = room.RoomId;

                await _userService.UpdateUser(user);

                await groups.AddToGroupAsync(context.ConnectionId, room.RoomId);

                RoomUserResponse response = new()
                {
                    Room = room,
                    User = user,
                };

                await caller.SendAsync("RoomCreated", response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleJoinRoom: {ex}"); // Log the exception
                await caller.SendAsync("RoomCreateFailed", ex.Message);
            }
        }

        public async Task HandleJoinRoom(HubCallerContext context, IHubCallerClients clients, IClientProxy caller, IGroupManager groups, string roomId, string userName)
        {
            try
            {
                User user = await _userService.CreateUser(userName, context.ConnectionId);
                Room room = await _roomService.JoinRoom(roomId, user);
                await _connectService.CreateConnection(context.ConnectionId, user.UserId.ToString(), room.RoomId);

                // update roomId
                user.RoomId = room.RoomId;
                await _userService.UpdateUser(user);

                RoomUserResponse response = new()
                {
                    Room = room,
                    User = user
                };

                await groups.AddToGroupAsync(context.ConnectionId, roomId);
                await clients.Group(roomId).SendAsync("GameJoined", response);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in HandleJoinRoom: {ex.Message}\n{ex.StackTrace}");
                await caller.SendAsync("JoinRoomFailed", ex.Message);
            }
        }

        public async Task HandleStartGame(IHubCallerClients clients, string roomId, string userId)
        {
            try
            {
                // validate room
                Room room = await _roomService.GetRoom(roomId);

                // validate user
                User user = await _userService.GetUser(userId);

                if (room.HostUserId != user.UserId)
                    throw new Exception("Only the host can start the game");

                if (room.Status != RoomStatus.Waiting)
                    throw new Exception("Game already started");

                // update room status & start game
                await _roomService.StartGame(roomId);

                await clients.Group(roomId).SendAsync("GameStarted");
            }
            catch (Exception ex)
            {
                await clients.Caller.SendAsync("GameStartFailed", ex.Message);
            }
        }

        public async Task HandleGetRound(IHubCallerClients clients, string roomId, string userId, int roundIndex)
        {
            try
            {
                // validate room
                Room room = await _roomService.GetRoom(roomId);

                // validate user
                User user = await _userService.GetUser(userId);

                if (room.HostUserId != user.UserId)
                    throw new Exception("Only the host can start the game");

                if (room.Status != RoomStatus.InProgress)
                    throw new Exception("Game isn't started or already ended!");

                // validate round index
                if (roundIndex < 0 || roundIndex >= room.Round)
                    throw new Exception("Invalid round index");

                // ensure round not reapeated
                if (roundIndex < room.CurrentRound)
                    throw new Exception("Round already ended!");

                // TODO: add check for current round already ended

                // get round info
                RoomTarget target = room.Targets[roundIndex];
                int delayBuffer = 1;
                DateTime endTime = DateTime.UtcNow + (room.TimeLimit + delayBuffer).Seconds();

                // update room endTime
                room.EndTime = endTime;
                room.CurrentRound = roundIndex;
                await _roomService.UpdateRoom(room);

                // create round object
                Round round = new Round
                {
                    TargetName = target.TargetName,
                    EndTime = endTime
                };

                await clients.Group(roomId).SendAsync("RoundInfo", round);
            }
            catch (Exception ex)
            {
                await clients.Caller.SendAsync("RoundInfoFailed", ex.Message);
            }
        }

        public async Task HandleSubmitImage(IHubCallerClients clients, string connectionId, string roomId, string userId, string base64Image)
        {
            try
            {
                // message queue
                await _roomEventService.EnqueueSubmitAsync(connectionId, roomId, userId, base64Image);
            }
            catch (Exception ex)
            {
                await clients.Caller.SendAsync("SubmitImageFailed", ex.Message);
            }
        }

        public async Task HandleGetRank(IHubCallerClients clients, string roomId, string userId)
        {
            try
            {
                List<Score> scores = await _rankService.GetRankInfo(roomId, userId);
                await clients.Group(roomId).SendAsync("RankInfo", scores);
            }
            catch (Exception ex)
            {
                await clients.Caller.SendAsync("RankFailed", ex.Message);
            }
        }

        public async Task HandleUserDisconnected(IHubCallerClients clients, string connectionId)
        {
            try
            {
                Connection? connection = await _connectService.GetUserIdFromConnectionId(connectionId);

                if (connection == null)
                    return;

                // get user from connection
                User user = await _userService.GetUser(connection.UserId);

                // delete connection
                await _connectService.DeleteConnection(connectionId);

                // delete user
                await _userService.DeleteUser(user.UserId.ToString());

                // update room record
                Room room = await _roomService.GetRoom(user.RoomId);
                await _roomService.RemoveUserFromRoom(room, user, connectionId);

                // send updated room to all users
                await clients.Group(room.RoomId).SendAsync("UserDisconnected", room);
            }
            catch (Exception ex)
            {
                await clients.Caller.SendAsync("UserDisconnectedFailed", ex.Message);
            }
        }
    }
}