namespace server.Models
{
    public enum RoomStatus
    {
        Waiting = 0,
        InProgress = 1,
        Finished = 2
    }

    public class Room
    {
        public required string RoomId { get; set; }
        public Guid HostUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Round { get; set; } = 5; // default: 5 rounds
        public int TimeLimit { get; set; } = 30; // default: 30 seconds
        public RoomStatus Status { get; set; } = RoomStatus.Waiting;
        // target items for identification
        public List<RoomTarget> Targets { get; set; } = new List<RoomTarget>();
        // relation to users
        public List<User> Users { get; set; } = new List<User>();
    }

    public class RoomTarget
    {
        public int Id { get; set; }
        public required string RoomId { get; set; }
        public required Room Room { get; set; }
        public int RoundIndex { get; set; }
        public required string TargetName { get; set; }
    }
}
