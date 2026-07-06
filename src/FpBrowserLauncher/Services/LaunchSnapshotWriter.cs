using System.IO;
using FpBrowserLauncher.Models;

namespace FpBrowserLauncher.Services;

public sealed class LaunchSnapshotWriter
{
    private readonly ProfileStore _profileStore;

    public LaunchSnapshotWriter(ProfileStore profileStore)
    {
        _profileStore = profileStore;
    }

    public async Task<string> WriteAsync(LaunchSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var directory = _profileStore.GetLaunchSnapshotsDirectory(snapshot.ProfileId);
        Directory.CreateDirectory(directory);

        var fileName = $"{snapshot.TimestampUtc:yyyyMMdd-HHmmss-fff}.json";
        var path = Path.Combine(directory, fileName);
        await JsonFile.WriteAsync(path, snapshot, cancellationToken);
        return path;
    }
}
