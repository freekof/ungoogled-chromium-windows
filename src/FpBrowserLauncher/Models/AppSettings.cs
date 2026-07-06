using System.Text.Json.Serialization;

namespace FpBrowserLauncher.Models;

public sealed class AppSettings
{
    [JsonPropertyName("chromium_path")]
    public string ChromiumPath { get; set; } = string.Empty;

    [JsonPropertyName("profiles_root_path")]
    public string ProfilesRootPath { get; set; } = "profiles";
}
