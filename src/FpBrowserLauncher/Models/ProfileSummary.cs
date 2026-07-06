namespace FpBrowserLauncher.Models;

public sealed class ProfileSummary
{
    public string ProfileId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ProfileDirectory { get; set; } = string.Empty;
    public string ProxySummary { get; set; } = string.Empty;
    public string FingerprintSummary { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public int? ProcessId { get; set; }

    public string DisplayText
    {
        get
        {
            var state = IsRunning ? $"运行中 PID {ProcessId}" : "已停止";
            var name = string.IsNullOrWhiteSpace(DisplayName) ? ProfileId : DisplayName;
            return $"{ProfileId} - {name} - {state}";
        }
    }
}
