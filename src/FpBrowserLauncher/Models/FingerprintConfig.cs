using System.Text.Json.Serialization;

namespace FpBrowserLauncher.Models;

public sealed class FingerprintConfig
{
    [JsonPropertyName("profile_id")]
    public string ProfileId { get; set; } = string.Empty;

    [JsonPropertyName("profile_seed")]
    public string ProfileSeed { get; set; } = string.Empty;

    [JsonPropertyName("user_agent")]
    public string UserAgent { get; set; } = string.Empty;

    [JsonPropertyName("webrtc")]
    public ModeValue WebRtc { get; set; } = new() { Mode = "proxy_udp" };

    [JsonPropertyName("timezone")]
    public ModeValue Timezone { get; set; } = new() { Mode = "based_on_ip", Value = "Asia/Shanghai" };

    [JsonPropertyName("geolocation")]
    public GeolocationValue Geolocation { get; set; } = new();

    [JsonPropertyName("language")]
    public ModeValue Language { get; set; } = new() { Mode = "based_on_ip", Value = "zh-CN" };

    [JsonPropertyName("ui_language")]
    public ModeValue UiLanguage { get; set; } = new() { Mode = "based_on_language", Value = "zh-CN" };

    [JsonPropertyName("resolution")]
    public ResolutionValue Resolution { get; set; } = new();

    [JsonPropertyName("fonts")]
    public FontsValue Fonts { get; set; } = new();

    [JsonPropertyName("noise_toggles")]
    public NoiseToggles NoiseToggles { get; set; } = new();

    [JsonPropertyName("webgl")]
    public WebGlValue WebGl { get; set; } = new();

    [JsonPropertyName("webgpu")]
    public ModeValue WebGpu { get; set; } = new() { Mode = "based_on_webgl" };

    [JsonPropertyName("cpu_cores")]
    public int CpuCores { get; set; }

    [JsonPropertyName("device_memory_gb")]
    public int DeviceMemoryGb { get; set; }

    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; } = string.Empty;

    [JsonPropertyName("mac_address")]
    public string MacAddress { get; set; } = string.Empty;

    [JsonPropertyName("do_not_track")]
    public string DoNotTrack { get; set; } = "default";

    [JsonPropertyName("port_scan_protection")]
    public PortScanProtection PortScanProtection { get; set; } = new();

    [JsonPropertyName("hardware_acceleration")]
    public string HardwareAcceleration { get; set; } = "default";

    [JsonPropertyName("tls_fingerprint")]
    public ModeValue TlsFingerprint { get; set; } = new() { Mode = "chrome_default" };

    [JsonPropertyName("extra_flags")]
    public List<string> ExtraFlags { get; set; } = ["--disable-notifications"];

    [JsonIgnore]
    public string Summary => $"{UserAgent}; {Resolution.Width}x{Resolution.Height}; {Timezone.Value}; {WebGl.Vendor}";
}

public sealed class ModeValue
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Value { get; set; }
}

public sealed class GeolocationValue
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "based_on_ip";

    [JsonPropertyName("prompt_policy")]
    public string PromptPolicy { get; set; } = "ask_every_time";
}

public sealed class ResolutionValue
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "based_on_ua";

    [JsonPropertyName("width")]
    public int Width { get; set; } = 1920;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 1080;
}

public sealed class FontsValue
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "custom";

    [JsonPropertyName("list")]
    public List<string> List { get; set; } = [];
}

public sealed class NoiseToggles
{
    [JsonPropertyName("canvas")]
    public bool Canvas { get; set; }

    [JsonPropertyName("webgl_image")]
    public bool WebGlImage { get; set; }

    [JsonPropertyName("audio_context")]
    public bool AudioContext { get; set; } = true;

    [JsonPropertyName("media_devices")]
    public bool MediaDevices { get; set; } = true;

    [JsonPropertyName("client_rects")]
    public bool ClientRects { get; set; } = true;

    [JsonPropertyName("speech_voices")]
    public bool SpeechVoices { get; set; } = true;
}

public sealed class WebGlValue
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "custom";

    [JsonPropertyName("vendor")]
    public string Vendor { get; set; } = string.Empty;

    [JsonPropertyName("renderer")]
    public string Renderer { get; set; } = string.Empty;
}

public sealed class PortScanProtection
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("allowed_ports")]
    public List<int> AllowedPorts { get; set; } = [];
}
