using System.Net;
using System.Text.Json;
using JyanTrackAPI.DTOs;

namespace JyanTrackAPI.Services;

public class AmaeKoromoService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<AmaeKoromoService> _logger;
    private const string FallbackFilePath = "player_records.json";

    public AmaeKoromoService(HttpClient httpClient, ILogger<AmaeKoromoService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://5-data.amae-koromo.com/api/v2/pl4/");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<PlayerSearchItem>> SearchPlayerAsync(string nickname)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"search_player/{Uri.EscapeDataString(nickname)}");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Referer", "https://amae-koromo.sapk.ch/");
            request.Headers.Add("Origin", "https://amae-koromo.sapk.ch");

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Search endpoint returned 429 Too Many Requests.");
                return new List<PlayerSearchItem>();
            }

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var players = await JsonSerializer.DeserializeAsync<List<PlayerSearchItem>>(stream, _jsonOptions);
            return players ?? new List<PlayerSearchItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search player {Nickname}.", nickname);
            return new List<PlayerSearchItem>();
        }
    }

    public async Task<List<MatchRecordDto>> GetRecentMatchesAsync(long accountId, string modes = "12,11,10,9")
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string url = $"player_records/{accountId}/{nowMs}/1262304000000?mode={modes}";

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Referer", "https://amae-koromo.sapk.ch/");
            request.Headers.Add("Origin", "https://amae-koromo.sapk.ch");

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Upstream API returned 429 Too Many Requests. Switching to fallback data.");
                return await LoadFallbackMatchesAsync();
            }

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var records = await JsonSerializer.DeserializeAsync<List<MatchRecordDto>>(stream, _jsonOptions);

            if (records != null && records.Any())
            {
                await File.WriteAllTextAsync(FallbackFilePath, JsonSerializer.Serialize(records, _jsonOptions));
                return records;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query live upstream API. Checking fallback cache.");
        }

        return await LoadFallbackMatchesAsync();
    }

    private async Task<List<MatchRecordDto>> LoadFallbackMatchesAsync()
    {
        if (File.Exists(FallbackFilePath))
        {
            var json = await File.ReadAllTextAsync(FallbackFilePath);
            var cached = JsonSerializer.Deserialize<List<MatchRecordDto>>(json, _jsonOptions);
            return cached ?? new List<MatchRecordDto>();
        }

        return new List<MatchRecordDto>();
    }
}