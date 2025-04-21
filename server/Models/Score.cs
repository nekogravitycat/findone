namespace server.Models
{
    public class Score
    {
        public Guid UserId { get; set; }
        public required string UserName { get; set; }
        public required double TotalRoundScore { get; set; }
        public required double CurrentRoundScore { get; set; }
        public string? Base64Image { get; set; }
        public string? Comment { get; set; }
    }
}
