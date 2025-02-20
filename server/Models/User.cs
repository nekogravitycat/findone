using server.Models;
using System;
using System.Collections.Generic;

namespace server.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public required string UserName { get; set; }

        // 外鍵連結至房間
        public required string RoomId { get; set; }
        public required Room Room { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // trust score for every round
        public List<UserScore>? Scores { get; set; }
    }

    public class UserScore
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public required User User { get; set; }
        // current round index
        public int RoundIndex { get; set; }
        public int Score { get; set; }
    }
}
