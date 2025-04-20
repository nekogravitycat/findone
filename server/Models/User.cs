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
        public required DateTime DateTime { get; set; }
        public Guid UserId { get; set; }
        public int RoundIndex { get; set; }
        public required string Base64Image { get; set; }
        public required string Comment { get; set; }
        public double Score { get; set; }
    }
}
