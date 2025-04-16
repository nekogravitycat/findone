namespace server.Models
{
    public class ImageRequest
    {
        public required string Base64Image { get; set; }
    }

    public class ImageResponse
    {
        public required Boolean Match { get; set; }
        public required string Comment { get; set; }
    }
}