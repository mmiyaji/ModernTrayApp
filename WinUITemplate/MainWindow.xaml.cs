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
    private const int ICON_SMALL = 0, ICON_BIG = 1;
    private const uint IMAGE_ICON = 1, LR_LOADFROMFILE = 0x10;

    private bool _isClosed = false;

    public MainWindow()
    {
        this.InitializeComponent();

        TrySetMicaBackdrop();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        SetWindowIcon();

        AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 700));
        CenterWindow();

        this.Activated += OnActivated;
        this.Closed += (s, e) =>
        {
            _isClosed = true;
            this.Activated -= OnActivated;
        };
    }

    private void OnActivated(object sender, WindowActivatedEventArgs e)
    {
        if (_isClosed) return;
        App.ApplyTheme(App.Settings.AppTheme);
    }

    /// <summary>
    /// ウィンドウ幅に応じてサイドバーの開閉・幅を動的に調整
    /// </summary>
    private void NavView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        double width = e.NewSize.Width;

        if (width < 500)
        {
            // 極小: オーバーレイモードで完全に隠す
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
        }
        else if (width < 860)
        {
            // 中: コンパクト（アイコンのみ）
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
        }
        else
        {
            // 大: フル展開
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
            // 幅に応じてペインの幅もスケール
            NavView.OpenPaneLength = width > 1200 ? 280 : 240;
        }
    }

    private void SetWindowIcon()
    {
        var exeDir = System.IO.Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        var icoPath = System.IO.Path.Combine(exeDir, "Assets", "AppIcon.ico");
        if (!System.IO.File.Exists(icoPath)) return;

        var hwnd = WindowNative.GetWindowHandle(this);
        var big = LoadImage(nint.Zero, icoPath, IMAGE_ICON, 32, 32, LR_LOADFROMFILE);
        if (big != nint.Zero) SendMessage(hwnd, WM_SETICON, ICON_BIG, big);
        var small = LoadImage(nint.Zero, icoPath, IMAGE_ICON, 16, 16, LR_LOADFROMFILE);
        if (small != nint.Zero) SendMessage(hwnd, WM_SETICON, ICON_SMALL, small);
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
        NavView.Header = title;
        if (ContentFrame.CurrentSourcePageType != pageType)
            ContentFrame.Navigate(pageType);
    }

    public void BringToFront()
    {
        if (_isClosed) return;
        AppWindow.Show();
        this.Activate();
        WindowHelper.BringToForeground(this);
    }
}