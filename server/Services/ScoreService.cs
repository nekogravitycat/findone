using Humanizer;
using server.Models;

namespace server.Services
{
    public class ScoreService
    {
        public double CalculateScore(Room room, DateTime submitTime)
        {
            DateTime roundEndTime = room.EndTime ?? throw new Exception("Round not started yet!");
            int timeLimitSeconds = room.TimeLimit;

            double secondsUsed = (submitTime - roundEndTime.AddSeconds(-timeLimitSeconds)).TotalSeconds;
            double timeLeft = Math.Max(0, timeLimitSeconds - secondsUsed);
            double scoreValue = (timeLeft / timeLimitSeconds) * 100.0;

            return scoreValue;
        }
    }
}
