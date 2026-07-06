using FpBrowserLauncher.Models;

namespace FpBrowserLauncher.Services;

public interface ISocks5ProxyTester
{
    Task<ProxyTestResult> TestAsync(ProxyConfig proxy, int timeoutMs = 5000, CancellationToken cancellationToken = default);
}

public sealed record ProxyTestResult(bool IsAlive, string Message)
{
    public static ProxyTestResult Alive() => new(true, "SOCKS5 代理可用。");
    public static ProxyTestResult Failed(string message) => new(false, message);
}
