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

        // 未処理例外をログに出力（デバッグ用）
        this.UnhandledException += (s, e) =>
        {
            Debug.WriteLine($"[UnhandledException] {e.Exception}");
            e.Handled = true; // クラッシュを防ぐ
        };
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
        // Closed 中はウィンドウ操作不可のため、次フレームに遅延
        var queue = (sender as MainWindow)?.DispatcherQueue;
        queue?.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            if (Settings.MinimizeToTray && Settings.EnableTrayIcon)
            {
                // 新しいウィンドウを準備（表示はトレイから）
                MainWindow = new MainWindow();
                MainWindow.Closed += OnMainWindowClosed;
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
            Debug.WriteLine($"[ShowMainWindow] {ex.Message} - 再生成します");
            MainWindow = new MainWindow();
            MainWindow.Closed += OnMainWindowClosed;
            MainWindow.Activate();
        }
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