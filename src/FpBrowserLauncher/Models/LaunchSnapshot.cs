using System.Text.Json.Serialization;

namespace FpBrowserLauncher.Models;

public sealed class LaunchSnapshot
{
    [JsonPropertyName("profile_id")]
    public string ProfileId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp_utc")]
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("chromium_path")]
    public string ChromiumPath { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public List<string> Arguments { get; set; } = [];

    [JsonPropertyName("process_id")]
    public int? ProcessId { get; set; }

    [JsonPropertyName("proxy_summary")]
    public string ProxySummary { get; set; } = string.Empty;

    [JsonPropertyName("fingerprint_summary")]
    public string FingerprintSummary { get; set; } = string.Empty;
}
