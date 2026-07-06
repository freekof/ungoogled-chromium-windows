using System.Windows;
using FpBrowserLauncher.ViewModels;

namespace FpBrowserLauncher;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
