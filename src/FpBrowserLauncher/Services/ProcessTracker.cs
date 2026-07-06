using System.Diagnostics;

namespace FpBrowserLauncher.Services;

public sealed class ProcessTracker
{
    private readonly Dictionary<string, Process> _running = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, int> RunningProcesses
    {
        get
        {
            lock (_running)
            {
                return _running.Where(pair => !pair.Value.HasExited).ToDictionary(pair => pair.Key, pair => pair.Value.Id, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    public void Track(string profileId, Process process)
    {
        lock (_running)
        {
            _running[profileId] = process;
        }

        process.EnableRaisingEvents = true;
        process.Exited += (_, _) =>
        {
            lock (_running)
            {
                _running.Remove(profileId);
            }
        };
    }

    public bool Stop(string profileId)
    {
        Process? process;
        lock (_running)
        {
            if (!_running.TryGetValue(profileId, out process))
            {
                return false;
            }
        }

        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }

        lock (_running)
        {
            _running.Remove(profileId);
        }

        return true;
    }

    public int? GetProcessId(string profileId)
    {
        lock (_running)
        {
            return _running.TryGetValue(profileId, out var process) && !process.HasExited ? process.Id : null;
        }
    }
}
