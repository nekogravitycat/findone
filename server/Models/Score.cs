namespace server.Models
{
    public class Score
    {
        public Guid UserId { get; set; }
        public required string UserName { get; set; }
        public required double ScoreValue { get; set; }
        public string? Base64Image { get; set; }
        public string? Comment { get; set; }
    }
}
