using FpBrowserLauncher.Services;
using Xunit;

namespace FpBrowserLauncher.Tests;

public sealed class FingerprintRandomizerTests
{
    [Fact]
    public void NextMacAddress_ReturnsSixHexBytes()
    {
        var mac = FingerprintRandomizer.NextMacAddress();

        Assert.Matches("^([0-9A-F]{2}-){5}[0-9A-F]{2}$", mac);
    }

    [Fact]
    public void NextWebGl_ReturnsConsistentVendorAndRendererBrand()
    {
        var webGl = FingerprintRandomizer.NextWebGl();

        Assert.Contains("Google Inc.", webGl.Vendor);
        Assert.Contains("ANGLE", webGl.Renderer);
        Assert.False(string.IsNullOrWhiteSpace(webGl.Renderer));
    }

    [Fact]
    public void NextGeolocation_ReturnsValidCoordinateRange()
    {
        var geo = FingerprintRandomizer.NextGeolocation();

        Assert.InRange(geo.Latitude, -90, 90);
        Assert.InRange(geo.Longitude, -180, 180);
        Assert.True(geo.Accuracy > 0);
    }
}
