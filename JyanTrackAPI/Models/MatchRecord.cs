namespace JyanTrackAPI.Models;

public class MatchRecord
{
    public int Id { get; set; }
    public string ExternalId{ get; set; } = string.Empty;
    public int RoomMode { get; set; } // 12 = Jade South, 10 = Gold South
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public List<MatchPlacement> Placements { get; set; } = new();
}