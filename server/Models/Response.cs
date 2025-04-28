using System.Text.Json.Serialization;

namespace server.Models
{
    public class Response
    {
        public class CreateRoomResponse
        {
            [JsonPropertyName("roomId")]
            public required string RoomId { get; set; }
            [JsonPropertyName("user")]
            public required User User { get; set; }
        }

        public class JoinRoomResponse
        {
            [JsonPropertyName("roomId")]
            public required string RoomId { get; set; }
            [JsonPropertyName("user")]
            public required User User { get; set; }
        }
    }
}
