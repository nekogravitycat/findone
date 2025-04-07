using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using server.Models;
using StackExchange.Redis;

namespace server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IConnectionMultiplexer _redis;

        public GameHub(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task CreateUser(string userName)
        {
            var user = await GenerateUser(userName);

            await Clients.Caller.SendAsync("UserCreated", user.UserId);
        }
        public async Task CreateRoom(string userName, int round, int timeLimit)
        {
            try
            {
                string roomId = GenerateRoomId();
                // todo: change
                string[] targets = { "Apple", "Banana", "Cherry", "Date", "Elderberry" };

                // create user
                var user = await GenerateUser(userName);

                var room = new Room
                {
                    RoomId = roomId,
                    HostUserId = user.UserId,
                    Round = round,
                    TimeLimit = timeLimit,
                    Targets = targets.Select((t, i) => new RoomTarget
                    {
                        RoomId = roomId,
                        Room = null!,
                        RoundIndex = i + 1,
                        TargetName = t
                    }).ToList()
                };
                var db = _redis.GetDatabase();
                var json = JsonSerializer.Serialize(room);

                // Save room to Redis for 2 hours
                await db.StringSetAsync($"room:{roomId}", json, TimeSpan.FromHours(2));

                // add room to group
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

                // return room id to the caller
                await Clients.Caller.SendAsync("RoomCreated", roomId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Error in CreateRoom: {ex.Message}");
            }
        }
        public async Task JoinRoom(string roomId, string userName)
        {
            try
            {
                var db = _redis.GetDatabase();

                // read room data from redis
                var roomJson = await db.StringGetAsync($"room:{roomId}");

                if (string.IsNullOrEmpty(roomJson))
                {
                    throw new Exception(
                        $"Room {roomId} does not exist or has been expired. Please create a new room.");
                }

                // if room exists, deserialize it or throw an exception
                var room = JsonSerializer.Deserialize<Room>(roomJson!) ?? throw new Exception(
                        $"Room {roomId} does not exist or has been expired. Please create a new room.");

                // create user & update room id
                var user = await GenerateUser(userName);
                user.RoomId = roomId;

                // add user to the room
                room.Users.Add(user);

                // update room data in Redis (expire in 2 hours)
                var updatedRoomJson = JsonSerializer.Serialize(room);
                await db.StringSetAsync($"room:{roomId}", updatedRoomJson, TimeSpan.FromHours(2));

                // add current connection to the group
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

                await Clients.Caller.SendAsync("GameJoined", roomId, user);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Error in JoinGame: {ex.Message}");
            }
        }
        public async Task StartGame(string roomId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var roomJson = await db.StringGetAsync($"room:{roomId}");

                if (string.IsNullOrEmpty(roomJson))
                {
                    throw new Exception($"Room {roomId} does not exist.");
                }

                var room = JsonSerializer.Deserialize<Room>(roomJson!) ?? throw new Exception($"Room {roomId} does is corrupted");

                room.Status = RoomStatus.InProgress;

                // reserialize room data and store into redis (default expired in 2hr)
                var updatedJson = JsonSerializer.Serialize(room);
                await db.StringSetAsync($"room:{roomId}", updatedJson, TimeSpan.FromHours(2));

                await Clients.Group(roomId).SendAsync("GameStarted", roomId, room);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Error in StartGame: {ex.Message}");
            }
        }
        public async Task CheckAnswer(string roomId, string userId, int roundIndex, string base64Image)
        {
            try
            {
                var db = _redis.GetDatabase();

                // ----- Get User -----
                var user = await GetUser($"user:{userId}");
                if (user == null) throw new Exception("User not found");

                // ---- Get Room -----
                var room = await GetRoom($"room:{roomId}");
                if (user == null) throw new Exception("Room not found");

                var current_target = room.Targets[roundIndex];

                // ----- todo: Computer Vision -----

                
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Error in CheckAnswer: {ex.Message}");
            }
        }
        private async Task<User> GetUser(string userKey)
        {
            var db = _redis.GetDatabase();
            var userJson = await db.StringGetAsync(userKey);
            if (string.IsNullOrEmpty(userJson))
            {
                throw new Exception($"user is not found");
            }

            var user = JsonSerializer.Deserialize<User>(userJson!) ?? throw new Exception($"useris not found");
            return user;
        }
        private async Task<Room> GetRoom(string roomKey)
        {
            var db = _redis.GetDatabase();
            var roomJson = await db.StringGetAsync(roomKey);
            if (string.IsNullOrEmpty(roomJson))
            {
                throw new Exception($"Room is not found");
            }
            var room = JsonSerializer.Deserialize<Room>(roomJson!) ?? throw new Exception($"Room is not found");
            return room;
        }
        private async Task<User> GenerateUser(string userName)
        {
            var user = new User { UserName = userName };

            // Save user to Redis for 2 hours
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(user);
            await db.StringSetAsync($"user:{user.UserId}", json, TimeSpan.FromHours(2));

            return user;
        }
        private static string GenerateRoomId(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
