using System.Text.Json.Serialization;

namespace server.Models
{
  public class ImageRequest
  {
    [JsonPropertyName("base64Image")]
    public required string Base64Image { get; set; }
  }

  public class ImageResponse
  {
    [JsonPropertyName("match")]
    public required Boolean Match { get; set; }

    [JsonPropertyName("comment")]
    public required string Comment { get; set; }
  }
}