using Humanizer;
using Microsoft.AspNetCore.SignalR;
using server.Models;

namespace server.Services
{

    public class GameService
    {
        private readonly RoomService _roomService;
        private readonly UserService _userService;
        private readonly ImageService _imageService;
        private readonly ScoreService _scoreService;

        public GameService(RoomService roomService, UserService userService, ImageService imageService, ScoreService scoreService)
        {
            _roomService = roomService;
            _userService = userService;
            _imageService = imageService;
            _scoreService = scoreService;
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

        public async Task HandleSubmitImage(IClientProxy caller, string userId, int roundIndex, string base64Image)
        {
            try
            {
                DateTime currentTime = DateTime.UtcNow;

                // validate user
                User user = await _userService.GetUser(userId);

                // validate room
                Room room = await _roomService.GetRoom(user.RoomId);

                if (room.Status != RoomStatus.InProgress)
                    throw new Exception("Game not started yet or already ended!");

                if (room.EndTime < currentTime)
                    throw new Exception("Round already closed!");

                if (room.CurrentRound != roundIndex)
                    throw new Exception("Invalid round index!");

                bool alreadySubmitted = user.Scores.Any(s => s.RoundIndex == roundIndex);
                if (alreadySubmitted)
                    throw new Exception("Image already submitted for this round!");

                // target of current round
                string target = room.Targets[roundIndex].TargetName;

                // analyze
                ImageResponse result = await _imageService.AnalyzeImage(base64Image, target) ?? throw new Exception("Image analysis failed");

                // check if image is correct
                if (result.Match == false)
                    throw new Exception("Image does not match the target");

                // update user
                UserScore user_score = new UserScore
                {
                    UserId = Guid.Parse(userId),
                    RoundIndex = roundIndex,
                    Comment = result.Comment,
                    DateTime = currentTime,
                    Base64Image = base64Image,
                    Score = _scoreService.CaclulateScore(room, currentTime)
                };

                // add score record to user
                await _userService.AddScore(user_score);

                // add submit record to room
                await _roomService.AddSubmit(userId, room.RoomId, currentTime, roundIndex);

                await caller.SendAsync("ImageAnalysisSuccessed");
            }
            catch (Exception ex)
            {
                await caller.SendAsync("ImageAnalysisFailed", ex.Message);
            }
        }

        public async Task HandleGetRank(IHubCallerClients clients, string roomId, string userId)
        {
            try
            {
                // validate room and user
                Room room = await _roomService.GetRoom(roomId);
                User user = await _userService.GetUser(userId);

                if (room.HostUserId != user.UserId)
                    throw new Exception("Only the host can start the game");

                int currentRound = room.CurrentRound ?? throw new Exception("Round not started yet!");
                DateTime roundEndTime = room.EndTime ?? throw new Exception("Round not started yet!");

                int timeLimitSeconds = room.TimeLimit;

                // get round info & all of users
                List<RoomSubmit> submits = room.RoomSubmits[currentRound];
                User[] allUsers = await _userService.GetUsers(room.UserIds.ToList());

                List<Score> scores = [];
                int i = 0;

                foreach (RoomSubmit submit in submits)
                {
                    User u = allUsers.FirstOrDefault(user => user.UserId == submit.UserId)
                        ?? throw new Exception("找不到對應的使用者");

                    // get user score
                    UserScore userScore = u.Scores.FirstOrDefault(s => s.RoundIndex == currentRound) ?? throw new Exception("找不到對應的分數");
                    double scoreValue = userScore.Score;

                    scores.Add(new Score
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        ScoreValue = scoreValue,
                        Base64Image = i < 3 && userScore != null ? userScore.Base64Image : "",
                        Comment = i < 3 && userScore != null ? userScore.Comment : ""
                    });

                    i++;
                }

                await clients.Group(roomId).SendAsync("RankInfo", scores);
            }
            catch (Exception ex)
            {
                await clients.Caller.SendAsync("RankFailed", ex.Message);
            }
        }
    }
}