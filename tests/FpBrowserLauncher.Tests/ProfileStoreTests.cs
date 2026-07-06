using FpBrowserLauncher.Models;
using FpBrowserLauncher.Services;

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
