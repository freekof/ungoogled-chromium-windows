using FpBrowserLauncher.Models;

namespace FpBrowserLauncher.Services;

public sealed class AppSettingsStore
{
    private readonly string _path;

    public AppSettingsStore(string? baseDirectory = null)
    {
        _path = Path.Combine(baseDirectory ?? AppContext.BaseDirectory, "appsettings.json");
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        var settings = await JsonFile.ReadAsync<AppSettings>(_path, cancellationToken);
        return settings ?? new AppSettings();
    }

    public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        return JsonFile.WriteAsync(_path, settings, cancellationToken);
    }
}
