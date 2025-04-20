
## Models

### User 相關模型

#### User
```csharp
public class User
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public required string UserName { get; init; }
    public string RoomId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public List<UserScore> Scores { get; set; } = new List<UserScore>();
}
```

#### UserScore
```csharp
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
```

### Room 相關模型

#### RoomStatus
```csharp
public enum RoomStatus
{
    Waiting = 0,
    InProgress = 1,
    Finished = 2
}
```

#### Room
```csharp
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
    public List<List<RoomSubmit>> RoomSubmits { get; set; } =
        Enumerable.Range(0, 5).Select(_ => new List<RoomSubmit>()).ToList();
}
```

#### RoomTarget
```csharp
public class RoomTarget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string RoomId { get; set; }
    public required string TargetName { get; set; }
}
```

#### RoomSubmit
```csharp
public class RoomSubmit
{
    public required DateTime DateTime { get; set; }
    public required Guid UserId { get; set; }
}
```

### Round 相關模型

#### Round
```csharp
namespace server.Models
{
    public class Round
    {
        public required string TargetName { get; set; }
        public DateTime EndTime { get; set; }
    }
}
```

### Score 相關模型

#### Score
```csharp
namespace server.Models
{
    public class Score
    {
        public Guid UserId { get; set; }
        public required string UserName { get; set; }
        public required double ScoreValue { get; set; }
        public string? Base64Image { get; set; }
        public string? Comment { get; set; }
    }
}
```