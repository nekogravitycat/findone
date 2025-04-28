using System.Text.Json.Serialization;

namespace server.Models
{
  public class Score
  {
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("userName")]
    public required string UserName { get; set; }

    [JsonPropertyName("totalRoundScore")]
    public required double TotalRoundScore { get; set; }

    [JsonPropertyName("currentRoundScore")]
    public required double CurrentRoundScore { get; set; }

    [JsonPropertyName("base64Image")]
    public string? Base64Image { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
  }
}
