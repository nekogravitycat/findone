namespace server.Models
{
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public required string UserName { get; init; }
        public string RoomId { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public List<UserScore> Scores { get; set; } = new List<UserScore>();
    }

    public class UserScore
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public Guid UserId { get; set; }
        public required User User { get; init; }
        public int RoundIndex { get; set; }
        public required string Comment { get; set; }
    }
}
