using Microsoft.AspNetCore.Mvc;
using JyanTrackAPI.Services;

namespace JyanTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MahjongStatsController : ControllerBase
{
    private readonly MahjongAnalyticsService _analyticsService;
    private readonly MatchSyncService _syncService;

    public MahjongStatsController(
        MahjongAnalyticsService analyticsService,
        MatchSyncService syncService)
    {
        _analyticsService = analyticsService;
        _syncService = syncService;
    }

    [HttpGet("profile/{accountId:long}")]
    public async Task<IActionResult> GetProfile(long accountId)
    {
        var stats = await _analyticsService.GetPlayerStatsAsync(accountId);
        if (stats == null)
        {
            return NotFound(new { message = $"No profile or matches found for Account ID: {accountId}" });
        }

        return Ok(stats);
    }

    [HttpPost("sync/{accountId:long}")]
    public async Task<IActionResult> SyncAndAnalyze(long accountId)
    {
        int imported = await _syncService.SyncPlayerMatchesAsync(accountId);
        var stats = await _analyticsService.GetPlayerStatsAsync(accountId);
        return Ok(new
        {
            importedCount = imported,
            stats
        });
    }
}