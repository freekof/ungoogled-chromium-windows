namespace FpBrowserLauncher.Models;

public sealed class LaunchResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? ProcessId { get; init; }

    public static LaunchResult Ok(int processId) => new()
    {
        Success = true,
        Message = $"已启动 Chromium，PID {processId}",
        ProcessId = processId
    };

    public static LaunchResult Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}
