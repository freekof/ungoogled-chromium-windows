using FpBrowserLauncher.Services;

namespace FpBrowserLauncher.Tests;

public sealed class ChromiumLauncherTests
{
    [Fact]
    public void FilterConflictingFlags_RemovesProtectedFlags()
    {
        var filtered = ChromiumLauncher.FilterConflictingFlags([
            "--user-data-dir=C:/leak",
            "--fp-config=C:/bad.json",
            "--proxy-server=socks5://127.0.0.1:1080",
            "--disable-notifications",
            " --lang=zh-CN "
        ]).ToList();

        Assert.DoesNotContain(filtered, flag => flag.StartsWith("--user-data-dir", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(filtered, flag => flag.StartsWith("--fp-config", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(filtered, flag => flag.StartsWith("--proxy-server", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("--disable-notifications", filtered);
        Assert.Contains("--lang=zh-CN", filtered);
    }

    [Fact]
    public void BuildCommandLineArguments_AddsProtectedProfileArgumentsFirst()
    {
        var root = Path.Combine(Path.GetTempPath(), "fp-launcher-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var store = new ProfileStore(root);
            var launcher = new ChromiumLauncher(store, new AlwaysAliveProxyTester(), new ProcessTracker(), new LaunchSnapshotWriter(store));
            var fingerprint = new FingerprintGenerator().Generate("p001");
            fingerprint.ExtraFlags = ["--disable-notifications", "--fp-config=C:/bad.json"];

            var args = launcher.BuildCommandLineArguments("p001", fingerprint);

            Assert.StartsWith("--user-data-dir=", args[0]);
            Assert.StartsWith("--fp-config=", args[1]);
            Assert.Contains("--no-first-run", args);
            Assert.Contains("--disable-background-networking", args);
            Assert.Contains("--disable-notifications", args);
            Assert.DoesNotContain(args.Skip(2), arg => arg.StartsWith("--fp-config=C:/bad.json", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
