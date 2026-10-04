namespace JyanTrackAPI.Models;

public class Player
{
    public int Id {get; set;}
    public long AccountId {get; set;}
    public string Nickname {get; set;} = string.Empty;
    public int DanLevel {get; set;}

    public List<MatchPlacement> Placements {get; set;} = new();
}