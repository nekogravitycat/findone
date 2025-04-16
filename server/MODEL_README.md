
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
    public int TimeLimit { get; set; } = 30; // default: 30 seconds
    public RoomStatus Status { get; set; } = RoomStatus.Waiting;
    // target items for identification
    public List<RoomTarget> Targets { get; set; } = new List<RoomTarget>();
    // relation to users
    public HashSet<Guid> UserIds { get; set; } = new HashSet<Guid>();
    // default : 5 rounds
    public List<RoomSubmit[]> RoomSubmits { get; set; } =
        Enumerable.Range(0, 5).Select(_ => new RoomSubmit[0]).ToList();
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