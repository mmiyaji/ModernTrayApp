using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using WinUITemplate.Helpers;
using WinUITemplate.Services;
using System;
using System.Diagnostics;

namespace WinUITemplate;

public partial class App : Application
{
    public static MainWindow? MainWindow { get; private set; }
    public static SettingsService Settings { get; private set; } = new();
    public static TrayIconService? TrayIcon { get; private set; }

    public App()
    {
        this.InitializeComponent();

        this.UnhandledException += (s, e) =>
        {
            Debug.WriteLine($"[UnhandledException] {e.Exception}");
            e.Handled = true;
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Settings.Load();

        TrayIcon = new TrayIconService();
        TrayIcon.ShowWindowRequested += ShowMainWindow;
        TrayIcon.ExitRequested += ExitApp;

        if (Settings.EnableTrayIcon)
            TrayIcon.Show();

        OpenMainWindow();
    }

    private static void OpenMainWindow()
    {
        MainWindow = new MainWindow();
        MainWindow.Closed += OnMainWindowClosed;
        MainWindow.Activate();
    }

    private static void OnMainWindowClosed(object sender, WindowEventArgs e)
    {
        var closedWindow = sender as MainWindow;
        if (closedWindow is not null)
            closedWindow.Closed -= OnMainWindowClosed;

        var queue = closedWindow?.DispatcherQueue;
        queue?.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            if (Settings.MinimizeToTray && Settings.EnableTrayIcon)
            {
                MainWindow = new MainWindow();
                MainWindow.Closed += OnMainWindowClosed;
                // トレイから表示するまで Activate しない
            }
            else
            {
                ExitApp();
            }
        });
    }

    public static void ShowMainWindow()
    {
        if (MainWindow is null)
        {
            MainWindow = new MainWindow();
            MainWindow.Closed += OnMainWindowClosed;
        }

        try
        {
            MainWindow.AppWindow.Show();
            MainWindow.Activate();
            WindowHelper.BringToForeground(MainWindow);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShowMainWindow] 再生成: {ex.Message}");
            MainWindow = new MainWindow();
            MainWindow.Closed += OnMainWindowClosed;
            MainWindow.Activate();
        }
    }

    public static void ApplyTheme(AppTheme theme)
    {
        try
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
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApplyTheme] スキップ: {ex.Message}");
        }
    }

    public static void ExitApp()
    {
        TrayIcon?.Dispose();
        Current.Exit();
    }
}
