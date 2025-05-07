using server.Models;
using static server.Models.Response;

namespace server.Services
{
    public class RankService
    {
        private readonly RoomService _roomService;
        private readonly UserService _userService;

        public RankService(RoomService roomService, UserService userService)
        {
            _roomService = roomService;
            _userService = userService;
        }

        public async Task<List<Score>> GetRankInfo(string roomId, string userId)
        {
            // validate room and user
            Room room = await _roomService.GetRoom(roomId);
            User user = await _userService.GetUser(userId);

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
                double totalRoundScore = u.Scores.Sum(s => s.Score);
                double currentRoundScore = userScore.Score;

                scores.Add(new Score
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    TotalRoundScore = totalRoundScore,
                    CurrentRoundScore = currentRoundScore,
                    Base64Image = i < 3 ? userScore.Base64Image : "",
                    Comment = i < 3 ? userScore.Comment : ""
                });

                i++;
            }

            // do decresing-order
            scores.Sort((a, b) => b.TotalRoundScore.CompareTo(a.TotalRoundScore));
            return scores;
        }
    }
} 