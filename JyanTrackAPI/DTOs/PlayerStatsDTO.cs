namespace JyanTrackAPI.DTOs;
public class PlayerStatsDto
{
    public long AccountId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int DanLevel { get; set; }
    public int TotalMatches { get; set; }

    // Placement Counts
    public int FirstPlaceCount { get; set; }
    public int SecondPlaceCount { get; set; }
    public int ThirdPlaceCount { get; set; }
    public int FourthPlaceCount { get; set; }

    // Placement Percentages
    public double FirstPlaceRate { get; set; }
    public double SecondPlaceRate { get; set; }
    public double ThirdPlaceRate { get; set; }
    public double FourthPlaceRate { get; set; }

    // Performance Indicators
    public double AverageRank { get; set; }
    public double LasAvoidanceRate { get; set; } // (1 - 4th_Rate): critical Jade Room metric
    public double AverageScore { get; set; }
    public int TotalUmaDelta { get; set; }
    public double PerformanceRating { get; set; } // Weighted skill index

    // Recent Form (Last 10 matches)
    public List<RecentMatchSummaryDto> RecentMatches { get; set; } = new();
}

public class RecentMatchSummaryDto
{
    public string ExternalId { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }
    public int Rank { get; set; }
    public int FinalScore { get; set; }
    public int UmaDelta { get; set; }
}