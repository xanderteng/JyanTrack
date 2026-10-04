using Microsoft.EntityFrameworkCore;
using JyanTrackAPI.Data;
using JyanTrackAPI.DTOs;
using JyanTrackAPI.Models;

namespace JyanTrackAPI.Services;

public class MatchSyncService
{
    private readonly AppDbContext _context;
    private readonly AmaeKoromoService _amaeService;
    private readonly ILogger<MatchSyncService> _logger;

    public MatchSyncService(AppDbContext context, AmaeKoromoService amaeService, ILogger<MatchSyncService> logger)
    {
        _context = context;
        _amaeService = amaeService;
        _logger = logger;
    }

    public async Task<int> SyncPlayerMatchesAsync(long accountId)
    {
        var rawMatches = await _amaeService.GetRecentMatchesAsync(accountId);
        if (!rawMatches.Any()) return 0;

        int addedCount = 0;

        foreach (var matchDto in rawMatches)
        {
            // Idempotency: Skip if match was already imported
            bool exists = await _context.MatchRecords.AnyAsync(m => m.ExternalId == matchDto.Uuid);
            if (exists) continue;

            var matchRecord = new MatchRecord
            {
                ExternalId = matchDto.Uuid,
                RoomMode = matchDto.Mode,
                StartTime = DateTimeOffset.FromUnixTimeSeconds(matchDto.StartTime).UtcDateTime,
                EndTime = DateTimeOffset.FromUnixTimeSeconds(matchDto.EndTime).UtcDateTime
            };

            // Order players by score descending to determine ranks (1st to 4th)
            var sortedPlayers = matchDto.Players.OrderByDescending(p => p.Score).ToList();

            for (int seatIndex = 0; seatIndex < matchDto.Players.Count; seatIndex++)
            {
                var playerDto = matchDto.Players[seatIndex];

                // Upsert Player record
                var player = await _context.Players.FirstOrDefaultAsync(p => p.AccountId == playerDto.AccountId);
                if (player == null)
                {
                    player = new Player
                    {
                        AccountId = playerDto.AccountId,
                        Nickname = playerDto.Nickname,
                        DanLevel = playerDto.Level
                    };
                    _context.Players.Add(player);
                    await _context.SaveChangesAsync(); // Commit to generate player.Id
                }
                else
                {
                    // Update latest Dan level and Nickname
                    player.Nickname = playerDto.Nickname;
                    player.DanLevel = playerDto.Level;
                }

                int rank = sortedPlayers.FindIndex(p => p.AccountId == playerDto.AccountId) + 1;

                var placement = new MatchPlacement
                {
                    MatchRecord = matchRecord,
                    Player = player,
                    Seat = seatIndex,
                    Rank = rank,
                    FinalScore = playerDto.Score,
                    UmaDelta = playerDto.GradingScore
                };

                matchRecord.Placements.Add(placement);
            }

            _context.MatchRecords.Add(matchRecord);
            addedCount++;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully synced {Count} new matches.", addedCount);
        return addedCount;
    }
}