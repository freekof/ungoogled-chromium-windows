using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace FpBrowserLauncher;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
        catch (Exception ex)
        {
            ReportStartupFailure(ex);
            MessageBox.Show(ex.Message, "FpBrowserLauncher 启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ReportStartupFailure(e.Exception);
        MessageBox.Show(e.Exception.Message, "FpBrowserLauncher 启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void ReportStartupFailure(Exception exception)
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "app-startup-error.log");
        File.AppendAllText(logPath, $"[{DateTimeOffset.Now:O}] {exception}{Environment.NewLine}");
    }
}
