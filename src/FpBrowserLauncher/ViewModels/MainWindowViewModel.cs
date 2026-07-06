using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FpBrowserLauncher.Models;
using FpBrowserLauncher.Services;

namespace FpBrowserLauncher.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly AppSettingsStore _settingsStore;
    private readonly ProfileStore _profileStore;
    private readonly ProcessTracker _processTracker;
    private readonly ChromiumLauncher _chromiumLauncher;

    private string _chromiumPath = string.Empty;
    private string _profileId = "p001";
    private string _displayName = "Profile 001";
    private string _proxyHost = "127.0.0.1";
    private string _proxyPortText = "1080";
    private string _proxyUsername = string.Empty;
    private string _proxyPassword = string.Empty;
    private string _statusMessage = "准备就绪。";
    private ProfileSummary? _selectedProfile;

    public MainWindowViewModel()
    {
        _settingsStore = new AppSettingsStore();
        var profilesRoot = Path.Combine(AppContext.BaseDirectory, "profiles");
        _profileStore = new ProfileStore(profilesRoot);
        _processTracker = new ProcessTracker();
        _chromiumLauncher = new ChromiumLauncher(
            _profileStore,
            new Socks5ProxyTester(),
            _processTracker,
            new LaunchSnapshotWriter(_profileStore));

        SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        SaveProfileCommand = new AsyncRelayCommand(SaveProfileAsync);
        DeleteProfileCommand = new AsyncRelayCommand(DeleteProfileAsync, () => SelectedProfile is not null);
        LaunchProfileCommand = new AsyncRelayCommand(LaunchProfileAsync, () => SelectedProfile is not null || !string.IsNullOrWhiteSpace(ProfileId));
        StopProfileCommand = new RelayCommand(StopProfile, () => SelectedProfile is not null);

        _ = InitializeAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ProfileSummary> Profiles { get; } = [];

    public AsyncRelayCommand SaveSettingsCommand { get; }
    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand SaveProfileCommand { get; }
    public AsyncRelayCommand DeleteProfileCommand { get; }
    public AsyncRelayCommand LaunchProfileCommand { get; }
    public RelayCommand StopProfileCommand { get; }

    public string ChromiumPath
    {
        get => _chromiumPath;
        set => SetField(ref _chromiumPath, value);
    }

    public string ProfileId
    {
        get => _profileId;
        set => SetField(ref _profileId, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetField(ref _displayName, value);
    }

    public string ProxyHost
    {
        get => _proxyHost;
        set => SetField(ref _proxyHost, value);
    }

    public string ProxyPortText
    {
        get => _proxyPortText;
        set => SetField(ref _proxyPortText, value);
    }

    public string ProxyUsername
    {
        get => _proxyUsername;
        set => SetField(ref _proxyUsername, value);
    }

    public string ProxyPassword
    {
        get => _proxyPassword;
        set => SetField(ref _proxyPassword, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public ProfileSummary? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (!SetField(ref _selectedProfile, value))
            {
                return;
            }

            if (value is not null)
            {
                _ = LoadSelectedProfileAsync(value.ProfileId);
            }

            OnPropertyChanged(nameof(SelectedProfileDetails));
            DeleteProfileCommand.RaiseCanExecuteChanged();
            LaunchProfileCommand.RaiseCanExecuteChanged();
            StopProfileCommand.RaiseCanExecuteChanged();
        }
    }

    public string SelectedProfileDetails => SelectedProfile is null
        ? "未选择 Profile。"
        : $"目录：{SelectedProfile.ProfileDirectory}{Environment.NewLine}代理：{SelectedProfile.ProxySummary}{Environment.NewLine}指纹：{SelectedProfile.FingerprintSummary}";

    private async Task InitializeAsync()
    {
        var settings = await _settingsStore.LoadAsync();
        ChromiumPath = settings.ChromiumPath;
        await RefreshAsync();
    }

    private async Task SaveSettingsAsync()
    {
        await _settingsStore.SaveAsync(new AppSettings { ChromiumPath = ChromiumPath, ProfilesRootPath = "profiles" });
        StatusMessage = "设置已保存。";
    }

    private async Task RefreshAsync()
    {
        var running = _processTracker.RunningProcesses;
        var profiles = await _profileStore.ListAsync();
        Profiles.Clear();

        foreach (var profile in profiles)
        {
            if (running.TryGetValue(profile.ProfileId, out var pid))
            {
                profile.IsRunning = true;
                profile.ProcessId = pid;
            }

            Profiles.Add(profile);
        }

        StatusMessage = $"已加载 {Profiles.Count} 个 Profile。";
    }

    private async Task SaveProfileAsync()
    {
        if (!int.TryParse(ProxyPortText, out var port))
        {
            StatusMessage = "代理端口必须是数字。";
            return;
        }

        var proxy = new ProxyConfig
        {
            Type = "socks5",
            Host = ProxyHost.Trim(),
            Port = port,
            Username = ProxyUsername.Trim(),
            Password = ProxyPassword
        };

        try
        {
            await _profileStore.CreateOrUpdateAsync(ProfileId, DisplayName, proxy);
            StatusMessage = $"Profile {ProfileId} 已保存并生成 fingerprint.json。";
            await RefreshAsync();
        }
        catch (ArgumentException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        if (_processTracker.GetProcessId(SelectedProfile.ProfileId) is not null)
        {
            StatusMessage = "Profile 正在运行，请先停止再删除。";
            return;
        }

        _profileStore.Delete(SelectedProfile.ProfileId);
        SelectedProfile = null;
        await RefreshAsync();
        StatusMessage = "Profile 已删除。";
    }

    private async Task LaunchProfileAsync()
    {
        var profileId = SelectedProfile?.ProfileId ?? ProfileId;
        var result = await _chromiumLauncher.LaunchAsync(profileId, ChromiumPath);
        StatusMessage = result.Message;
        await RefreshAsync();
    }

    private void StopProfile()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        StatusMessage = _processTracker.Stop(SelectedProfile.ProfileId) ? "已停止进程。" : "该 Profile 没有运行中的进程。";
        _ = RefreshAsync();
    }

    private async Task LoadSelectedProfileAsync(string profileId)
    {
        var metadata = await _profileStore.LoadMetadataAsync(profileId);
        var proxy = await _profileStore.LoadProxyAsync(profileId);

        ProfileId = profileId;
        DisplayName = metadata?.DisplayName ?? profileId;
        if (proxy is not null)
        {
            ProxyHost = proxy.Host;
            ProxyPortText = proxy.Port.ToString();
            ProxyUsername = proxy.Username;
            ProxyPassword = proxy.Password;
        }

        OnPropertyChanged(nameof(SelectedProfileDetails));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
