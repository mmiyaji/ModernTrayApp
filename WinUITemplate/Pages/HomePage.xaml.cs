using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;

namespace WinUITemplate.Pages;

public sealed partial class HomePage : Page
{
    private readonly List<CardItem> _cards = new()
    {
        new("機能 A", "説明テキストをここに入力します"),
        new("機能 B", "説明テキストをここに入力します"),
        new("機能 C", "説明テキストをここに入力します"),
        new("機能 D", "説明テキストをここに入力します"),
        new("機能 E", "説明テキストをここに入力します"),
        new("機能 F", "説明テキストをここに入力します"),
    };

    public HomePage()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) => RebuildGrid(RootPanel.ActualWidth);
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        RebuildGrid(e.NewSize.Width - 48);
    }

    private void RebuildGrid(double availableWidth)
    {
        if (availableWidth <= 0) return;

        const double idealItemWidth = 220.0;
        const double spacing = 12.0;

        int columns = Math.Max(1, (int)((availableWidth + spacing) / (idealItemWidth + spacing)));
        int rows = (int)Math.Ceiling((double)_cards.Count / columns);

        CardGrid.Children.Clear();
        CardGrid.ColumnDefinitions.Clear();
        CardGrid.RowDefinitions.Clear();
        CardGrid.ColumnSpacing = spacing;
        CardGrid.RowSpacing = spacing;

        for (int c = 0; c < columns; c++)
            CardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (int r = 0; r < rows; r++)
            CardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < _cards.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            var card = CreateCard(_cards[i]);
            Grid.SetColumn(card, col);
            Grid.SetRow(card, row);
            CardGrid.Children.Add(card);
        }
    }

    // 戻り値を FrameworkElement に変更
    private static FrameworkElement CreateCard(CardItem item)
    {
        var border = new Border
        {
            Style = (Style)Application.Current.Resources["CardBorderStyle"],
            MinHeight = 140,
        };
        var panel = new StackPanel { Padding = new Thickness(16), Spacing = 8 };
        panel.Children.Add(new FontIcon { FontSize = 32, Glyph = "\uE710" });
        panel.Children.Add(new TextBlock
        {
            Text = item.Title,
            Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"]
        });
        panel.Children.Add(new TextBlock
        {
            Text = item.Description,
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
            TextWrapping = TextWrapping.Wrap
        });
        border.Child = panel;
        return border;
    }
}

public record CardItem(string Title, string Description);