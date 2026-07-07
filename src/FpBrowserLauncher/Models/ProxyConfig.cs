using System.Text.Json.Serialization;

namespace FpBrowserLauncher.Models;

public sealed class ProxyConfig
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "socks5";

    [JsonPropertyName("host")]
    public string Host { get; set; } = "127.0.0.1";

    [JsonPropertyName("port")]
    public int Port { get; set; } = 1080;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("public_ip")]
    public string PublicIp { get; set; } = string.Empty;

    [JsonIgnore]
    public bool HasCredentials => !string.IsNullOrWhiteSpace(Username) || !string.IsNullOrWhiteSpace(Password);

    [JsonIgnore]
    public string Summary => $"{Type}://{Host}:{Port}";
}
