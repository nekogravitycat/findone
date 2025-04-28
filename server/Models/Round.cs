using System.Text.Json.Serialization;

namespace server.Models
{
  public class Round
  {
    [JsonPropertyName("targetName")]
    public required string TargetName { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }
  }
}
