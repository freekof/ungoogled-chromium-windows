using System.IO;
using FpBrowserLauncher.Models;
using FpBrowserLauncher.Services;
using Xunit;

namespace FpBrowserLauncher.Tests;

public sealed class ProfileStoreTests
{
    [Fact]
    public async Task CreateOrUpdateAsync_WritesProfileFiles()
    {
        var root = CreateTempDirectory();
        try
        {
            var store = new ProfileStore(root);

            await store.CreateOrUpdateAsync("p001", "Profile 001", new ProxyConfig
            {
                Host = "127.0.0.1",
                Port = 1080
            });

            Assert.True(File.Exists(store.GetMetadataPath("p001")));
            Assert.True(File.Exists(store.GetFingerprintPath("p001")));
            Assert.True(File.Exists(store.GetProxyPath("p001")));
            Assert.True(Directory.Exists(store.GetUserDataDirectory("p001")));
            Assert.True(Directory.Exists(store.GetLaunchSnapshotsDirectory("p001")));

            var profiles = await store.ListAsync();
            var profile = Assert.Single(profiles);
            Assert.Equal("p001", profile.ProfileId);
            Assert.Equal("Profile 001", profile.DisplayName);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task CreateOrUpdateAsync_SyncsProxyIntoFingerprintJson()
    {
        var root = CreateTempDirectory();
        try
        {
            var store = new ProfileStore(root);
            var proxy = new ProxyConfig
            {
                Host = "10.0.0.2",
                Port = 2080,
                Username = "user",
                Password = "pass",
                PublicIp = "203.0.113.10"
            };

            await store.CreateOrUpdateAsync("p001", "Profile 001", proxy);

            var fingerprint = await store.LoadFingerprintAsync("p001");
            Assert.NotNull(fingerprint);
            Assert.NotNull(fingerprint.Proxy);
            Assert.Equal("socks5", fingerprint.Proxy.Type);
            Assert.Equal("10.0.0.2", fingerprint.Proxy.Host);
            Assert.Equal(2080, fingerprint.Proxy.Port);
            Assert.Equal("user", fingerprint.Proxy.Username);
            Assert.Equal("pass", fingerprint.Proxy.Password);
            Assert.Equal("203.0.113.10", fingerprint.Proxy.PublicIp);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("../bad")]
    [InlineData("bad space")]
    [InlineData("")]
    public void ValidateProfileId_RejectsUnsafeValues(string profileId)
    {
        Assert.Throws<ArgumentException>(() => ProfileStore.ValidateProfileId(profileId));
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "fp-launcher-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
