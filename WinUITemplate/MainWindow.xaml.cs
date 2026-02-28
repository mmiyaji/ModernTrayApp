using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Runtime.InteropServices;
using WinRT.Interop;
using WinUITemplate.Helpers;
using WinUITemplate.Pages;

namespace WinUITemplate;

public sealed partial class MainWindow : Window
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern nint SendMessage(nint hWnd, int Msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern nint LoadImage(nint hInst, string lpszName, uint uType,
        int cxDesired, int cyDesired, uint fuLoad);

    private const int WM_SETICON = 0x0080;
    private const int ICON_SMALL = 0;
    private const int ICON_BIG = 1;
    private const uint IMAGE_ICON = 1;
    private const uint LR_LOADFROMFILE = 0x00000010;

    public MainWindow()
    {
        this.InitializeComponent();

        // Mica バックドロップ
        TrySetMicaBackdrop();

        // カスタムタイトルバー
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // ウィンドウアイコンを設定（タスクバー・Alt+Tab に反映）
        SetWindowIcon();

        // ウィンドウサイズ・位置
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 700));
        CenterWindow();

        this.Activated += (s, e) => App.ApplyTheme(App.Settings.AppTheme);
    }

    private void SetWindowIcon()
    {
        // .ico ファイルのパスを取得
        var exeDir = System.IO.Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        var icoPath = System.IO.Path.Combine(exeDir, "Assets", "AppIcon.ico");

        if (!System.IO.File.Exists(icoPath)) return;

        var hwnd = WindowNative.GetWindowHandle(this);

        // 大アイコン (32x32): タスクバー・Alt+Tab
        var hIconBig = LoadImage(nint.Zero, icoPath, IMAGE_ICON, 32, 32, LR_LOADFROMFILE);
        if (hIconBig != nint.Zero)
            SendMessage(hwnd, WM_SETICON, ICON_BIG, hIconBig);

        // 小アイコン (16x16): タイトルバー左上
        var hIconSmall = LoadImage(nint.Zero, icoPath, IMAGE_ICON, 16, 16, LR_LOADFROMFILE);
        if (hIconSmall != nint.Zero)
            SendMessage(hwnd, WM_SETICON, ICON_SMALL, hIconSmall);
    }

    private void TrySetMicaBackdrop()
    {
        if (MicaController.IsSupported())
            this.SystemBackdrop = new MicaBackdrop { Kind = MicaKind.Base };
        else
            this.SystemBackdrop = new DesktopAcrylicBackdrop();
    }

    private void CenterWindow()
    {
        var display = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary);
        var area = display.WorkArea;
        AppWindow.Move(new Windows.Graphics.PointInt32(
            (area.Width - 1100) / 2,
            (area.Height - 700) / 2));
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            NavigateTo(typeof(SettingsPage), "設定");
            return;
        }
        if (args.SelectedItem is NavigationViewItem item)
        {
            var (pageType, title) = item.Tag?.ToString() switch
            {
                "home" => (typeof(HomePage), "ホーム"),
                _ => (typeof(HomePage), "ホーム")
            };
            NavigateTo(pageType, title);
        }
    }

    private void NavigateTo(Type pageType, string title)
    {
        PageTitle.Text = title;
        if (ContentFrame.CurrentSourcePageType != pageType)
            ContentFrame.Navigate(pageType);
    }

    public void BringToFront()
    {
        AppWindow.Show();
        this.Activate();
        WindowHelper.BringToForeground(this);
    }
}