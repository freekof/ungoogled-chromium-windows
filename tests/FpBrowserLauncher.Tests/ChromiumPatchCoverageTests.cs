using System.IO;
using Xunit;

namespace FpBrowserLauncher.Tests;

public sealed class ChromiumPatchCoverageTests
{
    [Fact]
    public void FingerprintPatchesConsumeLauncherFingerprintSettings()
    {
        var patches = ReadFingerprintPatches();

        Assert.Contains("user_agent", patches);
        Assert.Contains("timezone.mode", patches);
        Assert.Contains("geolocation.mode", patches);
        Assert.Contains("geolocation.prompt_policy", patches);
        Assert.Contains("language.mode", patches);
        Assert.Contains("resolution.mode", patches);
        Assert.Contains("fonts.mode", patches);
        Assert.Contains("webgl.mode", patches);
        Assert.Contains("webgpu.mode", patches);
        Assert.Contains("disabled", patches);
        Assert.Contains("hardware_acceleration", patches);
    }

    private static string ReadFingerprintPatches()
    {
        var directory = FindRepositoryRoot(new DirectoryInfo(AppContext.BaseDirectory));
        var patchDirectory = Path.Combine(directory.FullName, "patches", "extra", "fp-browser");
        return string.Join('\n', Directory.GetFiles(patchDirectory, "*.patch").Select(File.ReadAllText));
    }

    private static DirectoryInfo FindRepositoryRoot(DirectoryInfo start)
    {
        for (var current = start; current is not null; current = current.Parent)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "patches", "extra", "fp-browser")))
            {
                return current;
            }
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing patches/extra/fp-browser.");
    }
}
