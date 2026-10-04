using System.Text.Json.Serialization;

namespace JyanTrackAPI.DTOs;

public class PlayerSearchItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public int Level { get; set; }
}