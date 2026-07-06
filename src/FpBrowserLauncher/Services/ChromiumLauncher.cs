using System.Diagnostics;
using FpBrowserLauncher.Models;

namespace FpBrowserLauncher.Services;

public sealed class ChromiumLauncher
{
    private static readonly string[] ProtectedFlagPrefixes =
    [
        "--user-data-dir",
        "--fp-config",
        "--proxy-server",
        "--host-resolver-rules"
    ];

    private readonly ProfileStore _profileStore;
    private readonly ISocks5ProxyTester _proxyTester;
    private readonly ProcessTracker _processTracker;
    private readonly LaunchSnapshotWriter _snapshotWriter;

    public ChromiumLauncher(ProfileStore profileStore, ISocks5ProxyTester proxyTester, ProcessTracker processTracker, LaunchSnapshotWriter snapshotWriter)
    {
        _profileStore = profileStore;
        _proxyTester = proxyTester;
        _processTracker = processTracker;
        _snapshotWriter = snapshotWriter;
    }

    public IReadOnlyList<string> BuildCommandLineArguments(string profileId, FingerprintConfig fingerprint)
    {
        var userDataDir = _profileStore.GetUserDataDirectory(profileId);
        var fpConfigPath = _profileStore.GetFingerprintPath(profileId);

        var args = new List<string>
        {
            $"--user-data-dir={userDataDir}",
            $"--fp-config={fpConfigPath}",
            "--no-first-run",
            "--disable-background-networking"
        };

        args.AddRange(FilterConflictingFlags(fingerprint.ExtraFlags));
        return args;
    }

    public static IEnumerable<string> FilterConflictingFlags(IEnumerable<string> extraFlags)
    {
        foreach (var flag in extraFlags.Where(flag => !string.IsNullOrWhiteSpace(flag)))
        {
            var trimmed = flag.Trim();
            if (ProtectedFlagPrefixes.Any(prefix => trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            yield return trimmed;
        }
    }

    public async Task<LaunchResult> LaunchAsync(string profileId, string chromiumPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(chromiumPath) || !File.Exists(chromiumPath))
        {
            return LaunchResult.Fail("Chromium 路径无效，已阻止启动。");
        }

        var fingerprint = await _profileStore.LoadFingerprintAsync(profileId, cancellationToken);
        if (fingerprint is null)
        {
            return LaunchResult.Fail("fingerprint.json 不存在或无法解析，已阻止启动。");
        }

        var proxy = await _profileStore.LoadProxyAsync(profileId, cancellationToken);
        if (proxy is null)
        {
            return LaunchResult.Fail("proxy.json 不存在或无法解析，已阻止启动。");
        }

        var proxyResult = await _proxyTester.TestAsync(proxy, 5000, cancellationToken);
        if (!proxyResult.IsAlive)
        {
            return LaunchResult.Fail($"代理不可用，已阻止启动：{proxyResult.Message}");
        }

        Directory.CreateDirectory(_profileStore.GetUserDataDirectory(profileId));
        var args = BuildCommandLineArguments(profileId, fingerprint);
        var startInfo = new ProcessStartInfo
        {
            FileName = chromiumPath,
            UseShellExecute = false
        };

        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        try
        {
            var process = Process.Start(startInfo);
            if (process is null)
            {
                return LaunchResult.Fail("Process.Start 未返回进程实例，已阻止标记为运行中。");
            }

            _processTracker.Track(profileId, process);

            await _snapshotWriter.WriteAsync(new LaunchSnapshot
            {
                ProfileId = profileId,
                ChromiumPath = chromiumPath,
                Arguments = args.ToList(),
                ProcessId = process.Id,
                ProxySummary = proxy.Summary,
                FingerprintSummary = fingerprint.Summary
            }, cancellationToken);

            return LaunchResult.Ok(process.Id);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
        {
            return LaunchResult.Fail($"Chromium 启动失败：{ex.Message}");
        }
    }
}
