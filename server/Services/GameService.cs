using Microsoft.AspNetCore.SignalR;
using server.Models;
using System.Diagnostics;

namespace server.Services
{

    public class GameService
    {
        private readonly RoomService _roomService;
        private readonly UserService _userService;
        private readonly ImageService _imageService;

        public GameService(RoomService roomService, UserService userService, ImageService imageService)
        {
            _roomService = roomService;
            _userService = userService;
            _imageService = imageService;
        }

        public async Task HandleGetUser(HubCallerContext context, IClientProxy caller, string userId)
        {
            try
            {
                User user = await _userService.GetUser(userId) ?? throw new Exception("User not found");
                await caller.SendAsync("UserFound", user);
            }
            catch (Exception ex)
            {
                await caller.SendAsync("UserNotFound", userId, ex.Message);
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
                await caller.SendAsync("RoomNotFound", roomId, ex.Message);
            }
        }

        public async Task HandleCreateRoom(HubCallerContext context, IClientProxy caller, IGroupManager groups, string userName, int round, int timeLimit)
        {
            User user = await _userService.CreateUser(userName);
            Room room = await _roomService.CreateRoom(user, round, timeLimit);

            user.RoomId = room.RoomId;

            await _userService.UpdateUser(user);

            await groups.AddToGroupAsync(context.ConnectionId, room.RoomId);
            await caller.SendAsync("RoomCreated", room.RoomId, user.UserId);
        }

        public async Task HandleJoinRoom(HubCallerContext context, IClientProxy caller, IGroupManager groups, string roomId, string userName)
        {
            User user = await _userService.CreateUser(userName);
            Room room = await _roomService.JoinRoom(roomId, user);

            // update roomId
            user.RoomId = room.RoomId;
            await _userService.UpdateUser(user);

            await groups.AddToGroupAsync(context.ConnectionId, roomId);
            await caller.SendAsync("GameJoined", roomId, user);
        }

        public async Task HandleStartGame(IHubCallerClients clients, string roomId)
        {
            Room room = await _roomService.StartGame(roomId);
            await clients.Group(roomId).SendAsync("GameStarted", roomId, room);
        }

        public async Task HandleSubmitImage(IClientProxy caller, string userId, int roundIndex, string base64Image)
        {
            try
            {
                DateTime currnetTime = DateTime.Now;

                // validate user
                User user = await _userService.GetUser(userId) ?? throw new Exception("User not found");

                // validate room
                Room room = await _roomService.GetRoom(user.RoomId) ?? throw new Exception("Room not found");

                // target of current round
                string target = room.Targets[roundIndex].TargetName;

                // analyze
                ImageResponse result = await _imageService.AnalyzeImage(base64Image, target) ?? throw new Exception("Image analysis failed");

                // check if image is correct
                if(result.Match == false)
                    throw new Exception("Image does not match the target");

                // update user
                UserScore user_score = new UserScore
                {
                    UserId = Guid.Parse(userId),
                    RoundIndex = roundIndex,
                    Comment = result.Comment,
                    DateTime = currnetTime,
                    Base64Image = base64Image
                };
                await _userService.AddScore(user_score);
                await _roomService.AddSubmit(userId, room.RoomId, currnetTime, roundIndex);

                await caller.SendAsync("ImageAnalysisSuccessed");
            }
            catch (Exception ex)
            {
                await caller.SendAsync("ImageAnalysisFailed", ex.Message);
            }
        }
    }
}