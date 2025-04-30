using System.Text.Json.Serialization;

namespace server.Models
{
    public class Response
    {
        public class RoomUserResponse
        {
            [JsonPropertyName("room")]
            public required Room Room { get; set; }
            [JsonPropertyName("user")]
            public required User User { get; set; }
        }
    }
}
