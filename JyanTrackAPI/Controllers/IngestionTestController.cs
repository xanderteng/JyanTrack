using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JyanTrackAPI.Data;
using JyanTrackAPI.Services;

namespace JyanTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngestionTestController : ControllerBase
{
    private readonly AmaeKoromoService _amaeService;
    private readonly MatchSyncService _syncService;
    private readonly AppDbContext _context;
    public IngestionTestController(
        AmaeKoromoService amaeService, 
        MatchSyncService syncService,
        AppDbContext context)
    {
        _amaeService = amaeService;
        _syncService = syncService;
        _context = context;
    }

    [HttpGet("player/{nickname}")]
    public async Task<IActionResult> FindPlayer(string nickname)
    {
        var results = await _amaeService.SearchPlayerAsync(nickname);
        if (!results.Any())
        {
            return NotFound($"No player found with nickname: {nickname}");
        }

        return Ok(results);
    }

    [HttpGet("matches/{accountId:long}")]
    public async Task<IActionResult> GetMatches(long accountId)
    {
        var records = await _amaeService.GetRecentMatchesAsync(accountId);
        return Ok(records);
    }

    [HttpPost("sync/{accountId:long}")]
    public async Task<IActionResult> Sync(long accountId)
    {
        int newMatches = await _syncService.SyncPlayerMatchesAsync(accountId);
        return Ok(new { message = "Sync completed", importedCount = newMatches });
    }

    [HttpGet("db/summary")]
    public async Task<IActionResult> GetDatabaseSummary()
    {
        var players = await _context.Players.ToListAsync();
        
        var matches = await _context.MatchRecords
            .Include(m => m.Placements)
                .ThenInclude(p => p.Player)
            .OrderByDescending(m => m.StartTime)
            .Take(5)
            .Select(m => new
            {
                m.Id,
                m.ExternalId,
                m.RoomMode,
                m.StartTime,
                Placements = m.Placements.Select(p => new
                {
                    PlayerName = p.Player != null ? p.Player.Nickname : "Unknown",
                    p.Seat,
                    p.Rank,
                    p.FinalScore,
                    p.UmaDelta
                }).OrderBy(p => p.Rank).ToList()
            })
            .ToListAsync();

        return Ok(new
        {
            totalPlayersInDb = players.Count,
            totalMatchesInDb = await _context.MatchRecords.CountAsync(),
            samplePlayers = players.Take(10),
            latestMatches = matches
        });
    }
}

