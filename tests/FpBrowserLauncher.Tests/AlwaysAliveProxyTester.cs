using FpBrowserLauncher.Models;
using FpBrowserLauncher.Services;

namespace FpBrowserLauncher.Tests;

internal sealed class AlwaysAliveProxyTester : ISocks5ProxyTester
{
    public Task<ProxyTestResult> TestAsync(ProxyConfig proxy, int timeoutMs = 5000, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ProxyTestResult.Alive());
    }
}
