using System.Text.Json.Serialization;

namespace server.Models
{
    public class Response
    {
        public class CreateRoomResponse
        {
            [JsonPropertyName("roomId")]
            public required string RoomId { get; set; }
            [JsonPropertyName("userId")]
            public Guid UserId { get; set; }
        }
    }
}
