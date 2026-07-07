using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
    private string _proxyPublicIp = string.Empty;
    private string _webRtcMode = "proxy_udp";
    private string _timezoneMode = "based_on_ip";
    private string _timezoneValue = "Asia/Shanghai";
    private string _geolocationMode = "based_on_ip";
    private string _geolocationPromptPolicy = "ask_every_time";
    private string _geolocationLatitude = string.Empty;
    private string _geolocationLongitude = string.Empty;
    private string _geolocationAccuracy = "1000";
    private string _languageMode = "based_on_ip";
    private string _languagesText = "zh-CN";
    private string _uiLanguageMode = "based_on_language";
    private string _uiLanguageValue = "zh-CN";
    private string _resolutionMode = "based_on_ua";
    private string _resolutionWidth = "1920";
    private string _resolutionHeight = "1080";
    private string _fontsMode = "custom";
    private string _fontsText = "Arial, Microsoft YaHei, SimSun, Calibri";
    private bool _canvasNoise;
    private bool _webGlImageNoise;
    private bool _audioContextNoise = true;
    private bool _mediaDevicesNoise = true;
    private bool _clientRectsNoise = true;
    private bool _speechVoicesNoise = true;
    private string _webGlMode = "custom";
    private string _webGlVendor = "Google Inc. (Intel)";
    private string _webGlRenderer = "ANGLE (Intel, Intel(R) HD Graphics Direct3D11 vs_5_0 ps_5_0)";
    private string _webGpuMode = "based_on_webgl";
    private string _cpuCores = "8";
    private string _deviceMemoryGb = "8";
    private string _deviceName = "DESKTOP-PROFILE";
    private string _macAddress = "00-50-43-3C-49-4B";
    private string _doNotTrack = "default";
    private bool _portScanProtectionEnabled = true;
    private string _allowedPorts = string.Empty;
    private string _hardwareAcceleration = "default";
    private string _tlsFingerprintMode = "chrome_default";
    private string _extraFlagsText = "--disable-notifications";
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
        RandomizeGeolocationCommand = new RelayCommand(RandomizeGeolocation);
        RandomizeResolutionCommand = new RelayCommand(RandomizeResolution);
        RandomizeFontsCommand = new RelayCommand(RandomizeFonts);
        RandomizeWebGlCommand = new RelayCommand(RandomizeWebGl);
        RandomizeCpuMemoryCommand = new RelayCommand(RandomizeCpuMemory);
        RandomizeDeviceNameCommand = new RelayCommand(RandomizeDeviceName);
        RandomizeMacAddressCommand = new RelayCommand(RandomizeMacAddress);

        _ = InitializeAsyncSafe();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ProfileSummary> Profiles { get; } = [];

    public AsyncRelayCommand SaveSettingsCommand { get; }
    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand SaveProfileCommand { get; }
    public AsyncRelayCommand DeleteProfileCommand { get; }
    public AsyncRelayCommand LaunchProfileCommand { get; }
    public RelayCommand StopProfileCommand { get; }
    public RelayCommand RandomizeGeolocationCommand { get; }
    public RelayCommand RandomizeResolutionCommand { get; }
    public RelayCommand RandomizeFontsCommand { get; }
    public RelayCommand RandomizeWebGlCommand { get; }
    public RelayCommand RandomizeCpuMemoryCommand { get; }
    public RelayCommand RandomizeDeviceNameCommand { get; }
    public RelayCommand RandomizeMacAddressCommand { get; }

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

    public string ProxyPublicIp { get => _proxyPublicIp; set => SetField(ref _proxyPublicIp, value); }
    public string WebRtcMode { get => _webRtcMode; set => SetField(ref _webRtcMode, value); }
    public string TimezoneMode { get => _timezoneMode; set => SetField(ref _timezoneMode, value); }
    public string TimezoneValue { get => _timezoneValue; set => SetField(ref _timezoneValue, value); }
    public string GeolocationMode { get => _geolocationMode; set => SetField(ref _geolocationMode, value); }
    public string GeolocationPromptPolicy { get => _geolocationPromptPolicy; set => SetField(ref _geolocationPromptPolicy, value); }
    public string GeolocationLatitude { get => _geolocationLatitude; set => SetField(ref _geolocationLatitude, value); }
    public string GeolocationLongitude { get => _geolocationLongitude; set => SetField(ref _geolocationLongitude, value); }
    public string GeolocationAccuracy { get => _geolocationAccuracy; set => SetField(ref _geolocationAccuracy, value); }
    public string LanguageMode { get => _languageMode; set => SetField(ref _languageMode, value); }
    public string LanguagesText { get => _languagesText; set => SetField(ref _languagesText, value); }
    public string UiLanguageMode { get => _uiLanguageMode; set => SetField(ref _uiLanguageMode, value); }
    public string UiLanguageValue { get => _uiLanguageValue; set => SetField(ref _uiLanguageValue, value); }
    public string ResolutionMode { get => _resolutionMode; set => SetField(ref _resolutionMode, value); }
    public string ResolutionWidth { get => _resolutionWidth; set => SetField(ref _resolutionWidth, value); }
    public string ResolutionHeight { get => _resolutionHeight; set => SetField(ref _resolutionHeight, value); }
    public string FontsMode { get => _fontsMode; set => SetField(ref _fontsMode, value); }
    public string FontsText { get => _fontsText; set => SetField(ref _fontsText, value); }
    public bool CanvasNoise { get => _canvasNoise; set => SetField(ref _canvasNoise, value); }
    public bool WebGlImageNoise { get => _webGlImageNoise; set => SetField(ref _webGlImageNoise, value); }
    public bool AudioContextNoise { get => _audioContextNoise; set => SetField(ref _audioContextNoise, value); }
    public bool MediaDevicesNoise { get => _mediaDevicesNoise; set => SetField(ref _mediaDevicesNoise, value); }
    public bool ClientRectsNoise { get => _clientRectsNoise; set => SetField(ref _clientRectsNoise, value); }
    public bool SpeechVoicesNoise { get => _speechVoicesNoise; set => SetField(ref _speechVoicesNoise, value); }
    public string WebGlMode { get => _webGlMode; set => SetField(ref _webGlMode, value); }
    public string WebGlVendor { get => _webGlVendor; set => SetField(ref _webGlVendor, value); }
    public string WebGlRenderer { get => _webGlRenderer; set => SetField(ref _webGlRenderer, value); }
    public string WebGpuMode { get => _webGpuMode; set => SetField(ref _webGpuMode, value); }
    public string CpuCores { get => _cpuCores; set => SetField(ref _cpuCores, value); }
    public string DeviceMemoryGb { get => _deviceMemoryGb; set => SetField(ref _deviceMemoryGb, value); }
    public string DeviceName { get => _deviceName; set => SetField(ref _deviceName, value); }
    public string MacAddress { get => _macAddress; set => SetField(ref _macAddress, value); }
    public string DoNotTrack { get => _doNotTrack; set => SetField(ref _doNotTrack, value); }
    public bool PortScanProtectionEnabled { get => _portScanProtectionEnabled; set => SetField(ref _portScanProtectionEnabled, value); }
    public string AllowedPorts { get => _allowedPorts; set => SetField(ref _allowedPorts, value); }
    public string HardwareAcceleration { get => _hardwareAcceleration; set => SetField(ref _hardwareAcceleration, value); }
    public string TlsFingerprintMode { get => _tlsFingerprintMode; set => SetField(ref _tlsFingerprintMode, value); }
    public string ExtraFlagsText { get => _extraFlagsText; set => SetField(ref _extraFlagsText, value); }

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

    private async Task InitializeAsyncSafe()
    {
        try
        {
            await InitializeAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"初始化失败：{ex.Message}";
        }
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
            Password = ProxyPassword,
            PublicIp = ProxyPublicIp.Trim()
        };

        try
        {
            await _profileStore.CreateOrUpdateAsync(ProfileId, DisplayName, proxy, ApplyFingerprintSettings);
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
        var fingerprint = await _profileStore.LoadFingerprintAsync(profileId);

        ProfileId = profileId;
        DisplayName = metadata?.DisplayName ?? profileId;
        if (proxy is not null)
        {
            ProxyHost = proxy.Host;
            ProxyPortText = proxy.Port.ToString();
            ProxyUsername = proxy.Username;
            ProxyPassword = proxy.Password;
            ProxyPublicIp = proxy.PublicIp;
        }

        if (fingerprint is not null)
        {
            LoadFingerprintSettings(fingerprint);
        }

        OnPropertyChanged(nameof(SelectedProfileDetails));
    }

    private void ApplyFingerprintSettings(FingerprintConfig fingerprint)
    {
        var languages = SplitCsv(LanguagesText);
        fingerprint.WebRtc = new ModeValue { Mode = WebRtcMode.Trim() };
        fingerprint.Timezone = new ModeValue { Mode = TimezoneMode.Trim(), Value = EmptyToNull(TimezoneValue) };
        fingerprint.Geolocation = new GeolocationValue
        {
            Mode = GeolocationMode.Trim(),
            PromptPolicy = GeolocationPromptPolicy.Trim(),
            Latitude = ParseNullableDouble(GeolocationLatitude),
            Longitude = ParseNullableDouble(GeolocationLongitude),
            Accuracy = ParseNullableDouble(GeolocationAccuracy)
        };
        fingerprint.Language = new ModeValue { Mode = LanguageMode.Trim(), Value = languages.FirstOrDefault() ?? EmptyToNull(LanguagesText) };
        fingerprint.Languages = languages;
        fingerprint.UiLanguage = new ModeValue { Mode = UiLanguageMode.Trim(), Value = EmptyToNull(UiLanguageValue) };
        fingerprint.Resolution = new ResolutionValue { Mode = ResolutionMode.Trim(), Width = ParseIntOrDefault(ResolutionWidth, 1920), Height = ParseIntOrDefault(ResolutionHeight, 1080) };
        fingerprint.Fonts = new FontsValue { Mode = FontsMode.Trim(), List = SplitCsv(FontsText) };
        fingerprint.NoiseToggles = new NoiseToggles { Canvas = CanvasNoise, WebGlImage = WebGlImageNoise, AudioContext = AudioContextNoise, MediaDevices = MediaDevicesNoise, ClientRects = ClientRectsNoise, SpeechVoices = SpeechVoicesNoise };
        fingerprint.WebGl = new WebGlValue { Mode = WebGlMode.Trim(), Vendor = WebGlVendor.Trim(), Renderer = WebGlRenderer.Trim() };
        fingerprint.WebGpu = new ModeValue { Mode = WebGpuMode.Trim() };
        fingerprint.CpuCores = ParseIntOrDefault(CpuCores, 8);
        fingerprint.DeviceMemoryGb = ParseIntOrDefault(DeviceMemoryGb, 8);
        fingerprint.DeviceName = DeviceName.Trim();
        fingerprint.MacAddress = MacAddress.Trim();
        fingerprint.DoNotTrack = DoNotTrack.Trim();
        fingerprint.PortScanProtection = new PortScanProtection { Enabled = PortScanProtectionEnabled, AllowedPorts = SplitCsv(AllowedPorts).Select(port => ParseIntOrDefault(port, -1)).Where(port => port > 0).ToList() };
        fingerprint.HardwareAcceleration = HardwareAcceleration.Trim();
        fingerprint.TlsFingerprint = new ModeValue { Mode = TlsFingerprintMode.Trim() };
        fingerprint.ExtraFlags = SplitLines(ExtraFlagsText);
    }

    private void LoadFingerprintSettings(FingerprintConfig fingerprint)
    {
        WebRtcMode = fingerprint.WebRtc.Mode;
        TimezoneMode = fingerprint.Timezone.Mode;
        TimezoneValue = fingerprint.Timezone.Value ?? string.Empty;
        GeolocationMode = fingerprint.Geolocation.Mode;
        GeolocationPromptPolicy = fingerprint.Geolocation.PromptPolicy;
        GeolocationLatitude = fingerprint.Geolocation.Latitude?.ToString() ?? string.Empty;
        GeolocationLongitude = fingerprint.Geolocation.Longitude?.ToString() ?? string.Empty;
        GeolocationAccuracy = fingerprint.Geolocation.Accuracy?.ToString() ?? string.Empty;
        LanguageMode = fingerprint.Language.Mode;
        var languageList = fingerprint.Languages.Count > 0
            ? fingerprint.Languages
            : new List<string> { fingerprint.Language.Value ?? string.Empty };
        LanguagesText = string.Join(", ", languageList);
        UiLanguageMode = fingerprint.UiLanguage.Mode;
        UiLanguageValue = fingerprint.UiLanguage.Value ?? string.Empty;
        ResolutionMode = fingerprint.Resolution.Mode;
        ResolutionWidth = fingerprint.Resolution.Width.ToString();
        ResolutionHeight = fingerprint.Resolution.Height.ToString();
        FontsMode = fingerprint.Fonts.Mode;
        FontsText = string.Join(", ", fingerprint.Fonts.List);
        CanvasNoise = fingerprint.NoiseToggles.Canvas;
        WebGlImageNoise = fingerprint.NoiseToggles.WebGlImage;
        AudioContextNoise = fingerprint.NoiseToggles.AudioContext;
        MediaDevicesNoise = fingerprint.NoiseToggles.MediaDevices;
        ClientRectsNoise = fingerprint.NoiseToggles.ClientRects;
        SpeechVoicesNoise = fingerprint.NoiseToggles.SpeechVoices;
        WebGlMode = fingerprint.WebGl.Mode;
        WebGlVendor = fingerprint.WebGl.Vendor;
        WebGlRenderer = fingerprint.WebGl.Renderer;
        WebGpuMode = fingerprint.WebGpu.Mode;
        CpuCores = fingerprint.CpuCores.ToString();
        DeviceMemoryGb = fingerprint.DeviceMemoryGb.ToString();
        DeviceName = fingerprint.DeviceName;
        MacAddress = fingerprint.MacAddress;
        DoNotTrack = fingerprint.DoNotTrack;
        PortScanProtectionEnabled = fingerprint.PortScanProtection.Enabled;
        AllowedPorts = string.Join(", ", fingerprint.PortScanProtection.AllowedPorts);
        HardwareAcceleration = fingerprint.HardwareAcceleration;
        TlsFingerprintMode = fingerprint.TlsFingerprint.Mode;
        ExtraFlagsText = string.Join(Environment.NewLine, fingerprint.ExtraFlags);
    }

    private void RandomizeGeolocation()
    {
        var geo = FingerprintRandomizer.NextGeolocation();
        GeolocationMode = "custom";
        GeolocationLatitude = geo.Latitude.ToString("F6");
        GeolocationLongitude = geo.Longitude.ToString("F6");
        GeolocationAccuracy = geo.Accuracy.ToString();
    }

    private void RandomizeResolution()
    {
        var resolution = FingerprintRandomizer.NextResolution();
        ResolutionMode = resolution.Mode;
        ResolutionWidth = resolution.Width.ToString();
        ResolutionHeight = resolution.Height.ToString();
    }

    private void RandomizeFonts()
    {
        FontsMode = "custom";
        FontsText = string.Join(", ", FingerprintRandomizer.NextFonts());
    }

    private void RandomizeWebGl()
    {
        var webGl = FingerprintRandomizer.NextWebGl();
        WebGlMode = webGl.Mode;
        WebGlVendor = webGl.Vendor;
        WebGlRenderer = webGl.Renderer;
    }

    private void RandomizeCpuMemory()
    {
        var hardware = FingerprintRandomizer.NextCpuAndMemory();
        CpuCores = hardware.CpuCores.ToString();
        DeviceMemoryGb = hardware.MemoryGb.ToString();
    }

    private void RandomizeDeviceName()
    {
        DeviceName = FingerprintRandomizer.NextDeviceName();
    }

    private void RandomizeMacAddress()
    {
        MacAddress = FingerprintRandomizer.NextMacAddress();
    }

    private static string? EmptyToNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int ParseIntOrDefault(string value, int defaultValue) => int.TryParse(value.Trim(), out var parsed) ? parsed : defaultValue;

    private static double? ParseNullableDouble(string value) => double.TryParse(value.Trim(), out var parsed) ? parsed : null;

    private static List<string> SplitCsv(string value) => value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

    private static List<string> SplitLines(string value) => value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

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
