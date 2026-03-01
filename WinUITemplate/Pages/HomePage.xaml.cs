using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using WinUITemplate.Controls;

namespace WinUITemplate.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();

        // ★ここだけ編集すればOK★
        CardGrid.Items = new List<CardItem>
        {
            new("機能 A", "説明テキストをここに入力します", "\uE710"),
            new("機能 B", "説明テキストをここに入力します", "\uE8A5"),
            new("機能 C", "説明テキストをここに入力します", "\uE734"),
            new("機能 D", "説明テキストをここに入力します", "\uE8F1"),
            new("機能 E", "説明テキストをここに入力します", "\uE713"),
            new("機能 F", "説明テキストをここに入力します", "\uE8FB"),
        };

        // クリック時の処理（任意）
        CardGrid.ItemClick = item =>
        {
            // item.Tag や item.Title で分岐
        };
    }
}
