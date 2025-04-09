namespace server.Models
{
    public class ImageRequest
    {
        public required string Base64Image { get; set; }
    }

    public class ImageResponse
    {
        public required string[] Object { get; set; }
        public required string[] Confidence { get; set; }
    }
}