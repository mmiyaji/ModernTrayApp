using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinUITemplate.Helpers;
using WinUITemplate.Services;

namespace WinUITemplate;

public partial class App : Application
{
    public static MainWindow? MainWindow { get; private set; }
    public static SettingsService Settings { get; private set; } = new();
    public static TrayIconService? TrayIcon { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Settings.Load();
        ApplyTheme(Settings.AppTheme);

        TrayIcon = new TrayIconService();
        TrayIcon.ShowWindowRequested += ShowMainWindow;
        TrayIcon.ExitRequested += ExitApp;

        if (Settings.EnableTrayIcon)
            TrayIcon.Show();

        MainWindow = new MainWindow();

        MainWindow.Closed += (s, e) =>
        {
            if (Settings.MinimizeToTray && Settings.EnableTrayIcon)
            {
                // WinUI 3 では Closed をキャンセルできないため、
                // 閉じる前に Hide して再生成する方式を取る
                MainWindow.AppWindow.Hide();
                // 新しいウィンドウを用意しておく（再表示用）
                MainWindow = new MainWindow();
            }
            else
            {
                ExitApp();
            }
        };

        MainWindow.Activate();
    }

    public static void ShowMainWindow()
    {
        if (MainWindow is null)
            MainWindow = new MainWindow();

        MainWindow.AppWindow.Show();
        MainWindow.Activate();
        WindowHelper.BringToForeground(MainWindow);
    }

    public static void ApplyTheme(AppTheme theme)
    {
        if (MainWindow?.Content is FrameworkElement root)
        {
            root.RequestedTheme = theme switch
            {
                AppTheme.Light => ElementTheme.Light,
                AppTheme.Dark => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }
    }

    public static void ExitApp()
    {
        TrayIcon?.Dispose();
        Current.Exit();
    }
}