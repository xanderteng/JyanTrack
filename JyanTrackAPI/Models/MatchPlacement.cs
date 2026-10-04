namespace JyanTrackAPI.Models;

public class MatchPlacement
{
    public int Id {get; set;}

    //FK to MatchRecord
    public int MatchRecordId {get; set;}
    public MatchRecord? MatchRecord {get; set;}

    //FK to Player
    public int PlayerId {get; set;}
    public Player? Player {get; set;}

    public int Seat {get; set;} // 0 = East, 1 = South, 2 = West, 3 = North
    public int Rank {get; set;} // 1st-4th
    public int FinalScore {get; set;} // e.g. 30000
    public int UmaDelta {get; set;} // Uma / Pt adjustment
}