using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using WinUITemplate.Pages;
using WinUITemplate.Helpers;
using System;

namespace WinUITemplate;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        // ウィンドウタイトルバーをカスタマイズ
        ExtendsContentIntoTitleBar = true;

        // ウィンドウサイズを設定
        var appWindow = this.AppWindow;
        appWindow.Resize(new Windows.Graphics.SizeInt32(1100, 700));
        appWindow.MoveAndResize(new Windows.Graphics.RectInt32(
            (int)(DisplayArea.Primary.WorkArea.Width - 1100) / 2,
            (int)(DisplayArea.Primary.WorkArea.Height - 700) / 2,
            1100, 700));

        // テーマを適用
        this.Activated += (s, e) => App.ApplyTheme(App.Settings.AppTheme);
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // 初期ページへ遷移
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
            var tag = item.Tag?.ToString();
            var (pageType, title) = tag switch
            {
                "home" => (typeof(HomePage), "ホーム"),
                // ここに追加ページを登録
                _ => (typeof(HomePage), "ホーム")
            };
            NavigateTo(pageType, title);
        }
    }

    private void NavigateTo(Type pageType, string title)
    {
        PageTitle.Text = title;
        ContentFrame.Navigate(pageType);
    }

    /// <summary>ウィンドウを前面に表示</summary>
    public void BringToFront()
    {
        this.AppWindow.Show();
        WindowHelper.BringToForeground(this);
    }
}
