namespace JyanTrackAPI.DTOs;

public class MatchRecordDto
{
    public string Uuid { get; set; } = string.Empty;
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public int Mode { get; set; }
    public List<MatchPlayerDto> Players { get; set; } = new();
}

public class MatchPlayerDto
{
    public long AccountId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Score { get; set; }
    public int GradingScore { get; set; }
}