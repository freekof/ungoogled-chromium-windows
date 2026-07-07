using FpBrowserLauncher.Models;

namespace FpBrowserLauncher.Services;

public static class FingerprintRandomizer
{
    private static readonly (string Vendor, string Renderer)[] WebGlPresets =
    [
        ("Google Inc. (Intel)", "ANGLE (Intel, Intel(R) UHD Graphics 620 Direct3D11 vs_5_0 ps_5_0)"),
        ("Google Inc. (Intel)", "ANGLE (Intel, Intel(R) HD Graphics 520 Direct3D11 vs_5_0 ps_5_0)"),
        ("Google Inc. (NVIDIA)", "ANGLE (NVIDIA, NVIDIA GeForce GTX 1660 Direct3D11 vs_5_0 ps_5_0)"),
        ("Google Inc. (NVIDIA)", "ANGLE (NVIDIA, NVIDIA GeForce RTX 3060 Direct3D11 vs_5_0 ps_5_0)"),
        ("Google Inc. (AMD)", "ANGLE (AMD, AMD Radeon RX 580 Direct3D11 vs_5_0 ps_5_0)")
    ];

    private static readonly (int Width, int Height)[] Resolutions =
    [
        (1366, 768),
        (1440, 900),
        (1600, 900),
        (1920, 1080),
        (2560, 1440)
    ];

    private static readonly string[][] FontPresets =
    [
        ["Arial", "Microsoft YaHei", "SimSun", "Calibri", "Segoe UI", "Times New Roman"],
        ["Arial", "Microsoft YaHei", "SimSun", "Calibri", "Segoe UI", "Verdana"],
        ["Arial", "Microsoft YaHei", "SimSun", "Calibri", "Segoe UI", "Tahoma"]
    ];

    private static readonly int[] CpuCores = [4, 6, 8, 12, 16, 20];
    private static readonly int[] MemoryGb = [8, 16, 32];

    public static WebGlValue NextWebGl()
    {
        var preset = WebGlPresets[Random.Shared.Next(WebGlPresets.Length)];
        return new WebGlValue { Mode = "custom", Vendor = preset.Vendor, Renderer = preset.Renderer };
    }

    public static ResolutionValue NextResolution()
    {
        var resolution = Resolutions[Random.Shared.Next(Resolutions.Length)];
        return new ResolutionValue { Mode = "custom", Width = resolution.Width, Height = resolution.Height };
    }

    public static IReadOnlyList<string> NextFonts()
    {
        return FontPresets[Random.Shared.Next(FontPresets.Length)];
    }

    public static (int CpuCores, int MemoryGb) NextCpuAndMemory()
    {
        return (CpuCores[Random.Shared.Next(CpuCores.Length)], MemoryGb[Random.Shared.Next(MemoryGb.Length)]);
    }

    public static string NextDeviceName()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var prefix = Random.Shared.Next(2) == 0 ? "DESKTOP" : "LAPTOP";
        var suffix = new string(Enumerable.Range(0, 7).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
        return $"{prefix}-{suffix}";
    }

    public static string NextMacAddress()
    {
        var bytes = new byte[6];
        Random.Shared.NextBytes(bytes);
        bytes[0] = (byte)((bytes[0] & 0b11111110) | 0b00000010);
        return string.Join('-', bytes.Select(value => value.ToString("X2")));
    }

    public static (double Latitude, double Longitude, int Accuracy) NextGeolocation()
    {
        var latitude = Random.Shared.NextDouble() * 180.0 - 90.0;
        var longitude = Random.Shared.NextDouble() * 360.0 - 180.0;
        var accuracy = new[] { 50, 100, 500, 1000 }[Random.Shared.Next(4)];
        return (Math.Round(latitude, 6), Math.Round(longitude, 6), accuracy);
    }
}
