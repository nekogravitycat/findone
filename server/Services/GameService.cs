using Microsoft.AspNetCore.SignalR;
using server.Models;

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
            var user = await _userService.GetUser(userId);
            if (user == null)
            {
                await caller.SendAsync("UserNotFound", userId);
                return;
            }
            await caller.SendAsync("UserFound", user);
        }

        public async Task HandleGetRoom(HubCallerContext context, IClientProxy caller, string roomId)
        {
            var room = await _roomService.GetRoom(roomId);
            if(room == null)
            {
                await caller.SendAsync("RoomNotFound", roomId);
                return;
            }
            await caller.SendAsync("RoomFound", room);
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

        public async Task HandleSubmitImage(IClientProxy caller, string roomId, string userId, int roundIndex, string base64Image)
        {
            try
            {
                // validate user
                User user = await _userService.GetUser(userId) ?? throw new Exception("User not found");

                // validate room
                Room room = await _roomService.GetRoom(roomId) ?? throw new Exception("Room not found");

                // analyze
                ImageResponse result = await _imageService.AnalyzeImage(base64Image) ?? throw new Exception("Image analysis failed");

                // check if image is correct
                float confidence = await _imageService.isImageCorrect(result, roomId, userId, roundIndex) ?? throw new Exception("Image is not correct");

                // update user
                UserScore user_score = new UserScore
                {
                    UserId = Guid.Parse(userId),
                    User = user,
                    RoundIndex = roundIndex,
                    Score = confidence
                };
                await _userService.AddScore(user_score);

                await caller.SendAsync("ImageAnalysisSuccessed");
            }
            catch (Exception ex)
            {
                await caller.SendAsync("ImageAnalysisFailed", ex.Message);
            }
        }
    }
}