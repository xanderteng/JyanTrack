using Microsoft.EntityFrameworkCore;
using JyanTrackAPI.Data;
using JyanTrackAPI.DTOs;

namespace JyanTrackAPI.Services;

public class MahjongAnalyticsService
{
    private readonly AppDbContext _context;

    public MahjongAnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlayerStatsDto?> GetPlayerStatsAsync(long accountId)
    {
        // 1. Fetch player by accountId
        var player = await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.AccountId == accountId);

        if (player == null) return null;

        // 2. Fetch all match placements for this player joined with match records
        var placements = await _context.MatchPlacements
            .AsNoTracking()
            .Where(mp => mp.PlayerId == player.Id)
            .Include(mp => mp.MatchRecord)
            .OrderByDescending(mp => mp.MatchRecord!.StartTime)
            .ToListAsync();

        int total = placements.Count;
        if (total == 0)
        {
            return new PlayerStatsDto
            {
                AccountId = player.AccountId,
                Nickname = player.Nickname,
                DanLevel = player.DanLevel,
                TotalMatches = 0
            };
        }

        // 3. LINQ aggregation queries
        int firsts = placements.Count(p => p.Rank == 1);
        int seconds = placements.Count(p => p.Rank == 2);
        int thirds = placements.Count(p => p.Rank == 3);
        int fourths = placements.Count(p => p.Rank == 4);

        double firstRate = (double)firsts / total * 100;
        double secondRate = (double)seconds / total * 100;
        double thirdRate = (double)thirds / total * 100;
        double fourthRate = (double)fourths / total * 100;

        double avgRank = placements.Average(p => p.Rank);
        double lasAvoidance = (1.0 - ((double)fourths / total)) * 100;
        double avgScore = placements.Average(p => p.FinalScore);
        int totalUma = placements.Sum(p => p.UmaDelta);

        // 4. Performance Rating Formula:
        // Baseline 1500 + (Rank Bonus: 1st=+30, 2nd=+10, 3rd=-10, 4th=-30 per match)
        double performanceRating = 1500.0 + ((firsts * 30.0 + seconds * 10.0 - thirds * 10.0 - fourths * 30.0) / total * 10.0);

        // 5. Recent 10 matches summary
        var recentMatches = placements
            .Take(10)
            .Select(p => new RecentMatchSummaryDto
            {
                ExternalId = p.MatchRecord?.ExternalId ?? string.Empty,
                PlayedAt = p.MatchRecord?.StartTime ?? DateTime.MinValue,
                Rank = p.Rank,
                FinalScore = p.FinalScore,
                UmaDelta = p.UmaDelta
            })
            .ToList();

        return new PlayerStatsDto
        {
            AccountId = player.AccountId,
            Nickname = player.Nickname,
            DanLevel = player.DanLevel,
            TotalMatches = total,

            FirstPlaceCount = firsts,
            SecondPlaceCount = seconds,
            ThirdPlaceCount = thirds,
            FourthPlaceCount = fourths,

            FirstPlaceRate = Math.Round(firstRate, 2),
            SecondPlaceRate = Math.Round(secondRate, 2),
            ThirdPlaceRate = Math.Round(thirdRate, 2),
            FourthPlaceRate = Math.Round(fourthRate, 2),

            AverageRank = Math.Round(avgRank, 2),
            LasAvoidanceRate = Math.Round(lasAvoidance, 2),
            AverageScore = Math.Round(avgScore, 1),
            TotalUmaDelta = totalUma,
            PerformanceRating = Math.Round(performanceRating, 1),

            RecentMatches = recentMatches
        };
    }
}