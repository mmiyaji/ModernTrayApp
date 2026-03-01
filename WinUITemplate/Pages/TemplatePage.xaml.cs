using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using WinUITemplate.Controls;

namespace WinUITemplate.Pages;

/// <summary>
/// 新しいページのひな形。
/// このファイルをコピーして使う場合:
///   1. クラス名・namespace を変更
///   2. MainWindow.xaml の MenuItems にアイテムを追加
///   3. MainWindow.xaml.cs の switch に遷移先を追加
/// </summary>
public sealed class TemplatePage : Page
{
    public TemplatePage()
    {
        // XAML を使わずコードだけでUIを構成
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };

        var outerGrid = new Grid();
        var panel = new StackPanel
        {
            Padding = new Thickness(24, 16, 24, 24),
            Spacing = 16,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        panel.Children.Add(new TextBlock
        {
            Text = "ページの説明をここに書いてください。",
            Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
        });

        // ★カードを使う場合: ここだけ編集★
        var cardGrid = new ResponsiveCardGrid
        {
            Items = new List<CardItem>
            {
                new("項目 A", "説明", "\uE710"),
                new("項目 B", "説明", "\uE8A5"),
            }
        };
        panel.Children.Add(cardGrid);

        // ★カードを使わない場合: 上の cardGrid を削除して好きなコントロールを追加★

        outerGrid.Children.Add(panel);
        scrollViewer.Content = outerGrid;
        this.Content = scrollViewer;
        this.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }
}
