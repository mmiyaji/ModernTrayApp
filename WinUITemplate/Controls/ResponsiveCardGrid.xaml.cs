using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;

namespace WinUITemplate.Controls;

public sealed partial class ResponsiveCardGrid : UserControl
{
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(IList<CardItem>),
            typeof(ResponsiveCardGrid),
            new PropertyMetadata(null, (d, _) => ((ResponsiveCardGrid)d).Rebuild()));

    public IList<CardItem> Items
    {
        get => (IList<CardItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public double IdealItemWidth { get; set; } = 220;
    public double ItemMinHeight { get; set; } = 120;
    public Action<CardItem>? ItemClick { get; set; }

    private readonly Grid _grid;

    public ResponsiveCardGrid()
    {
        _grid = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch };
        this.Content = _grid;
        this.SizeChanged += (s, e) => Rebuild();
    }

    private void Rebuild()
    {
        var items = Items;
        if (items is null || items.Count == 0) return;

        double available = this.ActualWidth;
        if (available <= 0) return;

        const double spacing = 12.0;
        int columns = Math.Max(1, (int)((available + spacing) / (IdealItemWidth + spacing)));
        int rows = (int)Math.Ceiling((double)items.Count / columns);

        _grid.Children.Clear();
        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();
        _grid.ColumnSpacing = spacing;
        _grid.RowSpacing = spacing;

        for (int c = 0; c < columns; c++)
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (int r = 0; r < rows; r++)
            _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < items.Count; i++)
        {
            var card = BuildCard(items[i]);
            Grid.SetColumn(card, i % columns);
            Grid.SetRow(card, i / columns);
            _grid.Children.Add(card);
        }
    }

    private FrameworkElement BuildCard(CardItem item)
    {
        var border = new Border
        {
            Style = (Style)Application.Current.Resources["CardBorderStyle"],
            MinHeight = ItemMinHeight,
        };

        if (ItemClick is not null)
        {
            border.PointerPressed += (s, e) => ItemClick(item);
            border.PointerEntered += (s, e) => border.Opacity = 0.8;
            border.PointerExited += (s, e) => border.Opacity = 1.0;
        }

        var panel = new StackPanel { Padding = new Thickness(16), Spacing = 8 };

        if (!string.IsNullOrEmpty(item.Glyph))
            panel.Children.Add(new FontIcon { FontSize = 28, Glyph = item.Glyph });

        panel.Children.Add(new TextBlock
        {
            Text = item.Title,
            Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"]
        });

        if (!string.IsNullOrEmpty(item.Description))
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

/// <summary>
/// カード1枚分のデータ。
/// record ではなく class にすることで XAML リフレクションのエラーを回避。
/// </summary>
public class CardItem
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Glyph { get; set; } = "\uE710";
    public object? Tag { get; set; }

    public CardItem() { }

    public CardItem(string title, string description = "", string glyph = "\uE710", object? tag = null)
    {
        Title = title;
        Description = description;
        Glyph = glyph;
        Tag = tag;
    }
}