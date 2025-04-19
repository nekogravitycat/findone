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
        public int? CurrentRound { get; set; } = null;
        public int TimeLimit { get; set; } = 30; // default: 30 seconds
        public DateTime? EndTime { get; set; } = null;
        public RoomStatus Status { get; set; } = RoomStatus.Waiting;
        // target items for identification
        public List<RoomTarget> Targets { get; set; } = new List<RoomTarget>();
        // relation to users
        public HashSet<Guid> UserIds { get; set; } = new HashSet<Guid>();
        // default : 5 rounds
        public List<RoomSubmit[]> RoomSubmits { get; set; } =
            Enumerable.Range(0, 5).Select(_ => new RoomSubmit[0]).ToList();
    }

    public class RoomTarget
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string RoomId { get; set; }
        public required string TargetName { get; set; }
    }

    public class RoomSubmit
    {
        public required DateTime DateTime { get; set; }
        public required Guid UserId { get; set; }
    }
}
