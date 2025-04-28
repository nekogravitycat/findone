using System.Text.Json.Serialization;

namespace server.Models {
  public class User
  {
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [JsonPropertyName("userName")]
    public required string UserName { get; init; }

    [JsonPropertyName("roomId")]
    public string RoomId { get; set; } = string.Empty;

    [JsonPropertyName("joinedAt")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("scores")]
    public List<UserScore> Scores { get; set; } = new List<UserScore>();
  }

  public class UserScore
  {
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("dateTime")]
    public required DateTime DateTime { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("roundIndex")]
    public int RoundIndex { get; set; }

    [JsonPropertyName("base64Image")]
    public required string Base64Image { get; set; }

    [JsonPropertyName("comment")]
    public required string Comment { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }
  }
}
