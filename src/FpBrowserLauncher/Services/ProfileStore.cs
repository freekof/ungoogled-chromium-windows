using System.Text.RegularExpressions;
using FpBrowserLauncher.Models;

namespace FpBrowserLauncher.Services;

public sealed partial class ProfileStore
{
    private readonly FingerprintGenerator _fingerprintGenerator;

    public ProfileStore(string rootPath, FingerprintGenerator? fingerprintGenerator = null)
    {
        RootPath = Path.GetFullPath(rootPath);
        _fingerprintGenerator = fingerprintGenerator ?? new FingerprintGenerator();
    }

    public string RootPath { get; }

    public string GetProfileDirectory(string profileId) => Path.Combine(RootPath, ValidateProfileId(profileId));
    public string GetMetadataPath(string profileId) => Path.Combine(GetProfileDirectory(profileId), "profile.json");
    public string GetFingerprintPath(string profileId) => Path.Combine(GetProfileDirectory(profileId), "fingerprint.json");
    public string GetProxyPath(string profileId) => Path.Combine(GetProfileDirectory(profileId), "proxy.json");
    public string GetUserDataDirectory(string profileId) => Path.Combine(GetProfileDirectory(profileId), "user_data");
    public string GetLaunchSnapshotsDirectory(string profileId) => Path.Combine(GetProfileDirectory(profileId), "launch-snapshots");

    public async Task<IReadOnlyList<ProfileSummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(RootPath);
        var summaries = new List<ProfileSummary>();

        foreach (var directory in Directory.EnumerateDirectories(RootPath).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var profileId = Path.GetFileName(directory);
            var metadata = await LoadMetadataAsync(profileId, cancellationToken);
            var proxy = await LoadProxyAsync(profileId, cancellationToken);
            var fingerprint = await LoadFingerprintAsync(profileId, cancellationToken);

            summaries.Add(new ProfileSummary
            {
                ProfileId = profileId,
                DisplayName = metadata?.DisplayName ?? profileId,
                ProfileDirectory = directory,
                ProxySummary = proxy?.Summary ?? "未配置代理",
                FingerprintSummary = fingerprint?.Summary ?? "未生成指纹"
            });
        }

        return summaries;
    }

    public async Task CreateOrUpdateAsync(string profileId, string displayName, ProxyConfig proxy, CancellationToken cancellationToken = default)
    {
        profileId = ValidateProfileId(profileId);
        var directory = GetProfileDirectory(profileId);
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(GetUserDataDirectory(profileId));
        Directory.CreateDirectory(GetLaunchSnapshotsDirectory(profileId));

        var existingMetadata = await LoadMetadataAsync(profileId, cancellationToken);
        var metadata = new ProfileMetadata
        {
            ProfileId = profileId,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? profileId : displayName.Trim(),
            CreatedAtUtc = existingMetadata?.CreatedAtUtc ?? DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var fingerprint = _fingerprintGenerator.Generate(profileId);
        await JsonFile.WriteAsync(GetMetadataPath(profileId), metadata, cancellationToken);
        await JsonFile.WriteAsync(GetProxyPath(profileId), proxy, cancellationToken);
        await JsonFile.WriteAsync(GetFingerprintPath(profileId), fingerprint, cancellationToken);
    }

    public Task<ProfileMetadata?> LoadMetadataAsync(string profileId, CancellationToken cancellationToken = default)
    {
        return JsonFile.ReadAsync<ProfileMetadata>(GetMetadataPath(profileId), cancellationToken);
    }

    public Task<ProxyConfig?> LoadProxyAsync(string profileId, CancellationToken cancellationToken = default)
    {
        return JsonFile.ReadAsync<ProxyConfig>(GetProxyPath(profileId), cancellationToken);
    }

    public Task<FingerprintConfig?> LoadFingerprintAsync(string profileId, CancellationToken cancellationToken = default)
    {
        return JsonFile.ReadAsync<FingerprintConfig>(GetFingerprintPath(profileId), cancellationToken);
    }

    public void Delete(string profileId)
    {
        var directory = GetProfileDirectory(profileId);
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    public static string ValidateProfileId(string profileId)
    {
        var trimmed = profileId.Trim();
        if (!ProfileIdPattern().IsMatch(trimmed))
        {
            throw new ArgumentException("Profile ID 只能包含英文字母、数字、下划线和短横线。", nameof(profileId));
        }

        return trimmed;
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{1,64}$")]
    private static partial Regex ProfileIdPattern();
}
