using System.Text.Json.Serialization;

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
    [JsonPropertyName("roomId")]
    public required string RoomId { get; set; }

    [JsonPropertyName("hostUserId")]
    public Guid HostUserId { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("round")]
    public int Round { get; set; } = 5; // default: 5 rounds

    [JsonPropertyName("currentRound")]
    public int? CurrentRound { get; set; } = null;

    [JsonPropertyName("timeLimit")]
    public int TimeLimit { get; set; } = 30; // default: 30 seconds

    [JsonPropertyName("endTime")]
    public DateTime? EndTime { get; set; } = null;

    [JsonPropertyName("status")]
    public RoomStatus Status { get; set; } = RoomStatus.Waiting;

    [JsonPropertyName("targets")]
    public List<RoomTarget> Targets { get; set; } = new List<RoomTarget>();

    [JsonPropertyName("userIds")]
    public HashSet<Guid> UserIds { get; set; } = new HashSet<Guid>();

    [JsonPropertyName("userConnections")]
    public HashSet<string> UserConnections { get; set; } = new HashSet<string>();

    [JsonPropertyName("roomSubmits")]
    public List<List<RoomSubmit>> RoomSubmits { get; set; } =
      Enumerable.Range(0, 5).Select(_ => new List<RoomSubmit>()).ToList();
  }

  public class RoomTarget
  {
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("roomId")]
    public required string RoomId { get; set; }

    [JsonPropertyName("targetName")]
    public required string TargetName { get; set; }
  }

  public class RoomSubmit
  {
    [JsonPropertyName("dateTime")]
    public required DateTime DateTime { get; set; }

    [JsonPropertyName("userId")]
    public required Guid UserId { get; set; }
  }
}
