# WinUI Template App

WinUI 3 製アプリを素早く量産するためのテンプレートです。  
NavigationView・設定画面・タスクトレイ常駐・Mica 背景・レスポンシブレイアウトを標準搭載しています。

---

## 機能一覧

| 機能 | 説明 |
|------|------|
| NavigationView | 左ペイン・コンパクト・オーバーレイをウィンドウ幅に応じて自動切替 |
| Mica 背景 | Windows 11 は Mica、Windows 10 は Acrylic にフォールバック |
| カスタムタイトルバー | アイコン＋アプリ名を表示。ドラッグ可能 |
| テーマ切替 | ライト / ダーク / システムに合わせる |
| タスクトレイ常駐 | Win32 P/Invoke による軽量実装（WinForms 不使用） |
| 閉じるボタンでトレイ最小化 | × ボタンで終了せずトレイへ格納 |
| 自動起動 | レジストリ `HKCU\...\Run` への登録・解除 |
| レスポンシブカードグリッド | ウィンドウ幅に応じて列数を自動調整 |
| 設定の永続化 | JSON ファイルに保存（`%LocalAppData%\WinUITemplate\settings.json`） |
| アンパッケージド実行 | MSIX 不要。exe を直接実行可能 |

---

## 動作要件

| 項目 | バージョン |
|------|-----------|
| OS | Windows 10 (19041) 以降 |
| .NET | 8.0 |
| Windows App SDK | 1.8.260209005 |
| Visual Studio | 2022 17.x 以降 |
| ワークロード | `.NET デスクトップ開発` + `Windows アプリケーション開発` |

---

## プロジェクト構成
```
WinUITemplate/
├── App.xaml / App.xaml.cs             # エントリポイント、テーマ・トレイ初期化
├── MainWindow.xaml / .cs              # NavigationView、Mica、カスタムタイトルバー
├── Pages/
│   ├── HomePage.xaml / .cs            # ホーム画面（カードグリッド）
│   ├── SettingsPage.xaml / .cs        # 設定画面
│   └── TemplatePage.xaml.cs           # 新ページのひな形（XAML なし）
├── Controls/
│   └── ResponsiveCardGrid.xaml / .cs  # レスポンシブカードグリッド
├── Services/
│   ├── SettingsService.cs             # 設定の読み書き（JSON）
│   ├── TrayIconService.cs             # タスクトレイ（Win32 P/Invoke）
│   └── AutoStartService.cs            # 自動起動（レジストリ）
├── Helpers/
│   └── WindowHelper.cs                # Win32 SetForegroundWindow
├── Styles/
│   └── AppStyles.xaml                 # CardBorderStyle など共通スタイル
└── Assets/
    ├── AppIcon.ico                     # タスクバー・トレイアイコン
    └── AppIcon.png                     # タイトルバーアイコン
```

---

## テンプレートの展開手順

### 1. フォルダをコピー

`WinUITemplate/` フォルダをそのままコピーして新しい名前に変更します。
```
WinUITemplate/  →  MyApp/
```

### 2. アプリ名を一括置換

Visual Studio の **編集 → 検索と置換 → フォルダー内を置換**（`Ctrl+Shift+H`）で以下を実行します。

| 検索 | 置換後 | 対象ファイル |
|------|--------|-------------|
| `WinUITemplate` | `MyApp` | `*.cs` `*.xaml` `*.csproj` `*.sln` |
| `WinUI Template App` | `My App Name` | `*.cs` `*.xaml` |

> **ファイル名もリネームしてください**  
> `WinUITemplate.csproj` → `MyApp.csproj`  
> `WinUITemplate.sln` → `MyApp.sln`

### 3. アイコンを差し替える

`Assets/` フォルダの以下のファイルを差し替えます。

| ファイル | 用途 |
|----------|------|
| `AppIcon.ico` | タスクバー・タスクトレイ・タイトルバー左上 |
| `AppIcon.png` | NavigationView ヘッダー内のアイコン |

ICO ファイルには 16 / 32 / 48 / 64 / 128 / 256px を含めてください。

### 4. ウィンドウタイトルを変更

`MainWindow.xaml` の `Title` 属性を変更します。
```xml
<Window Title="My App Name" ...>
```

### 5. 設定ファイルの保存先を変更

`SettingsService.cs` のパスを変更します。
```csharp
private static readonly string _settingsPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MyApp",          // ← アプリ名に変更
    "settings.json");
```

### 6. 自動起動のレジストリキー名を変更

`AutoStartService.cs` のキー名を変更します。
```csharp
private const string AppName = "MyApp";  // ← アプリ名に変更
```

---

## 新しいページの追加

**1. `TemplatePage.xaml.cs` をコピー**してクラス名・namespace を変更します。
```csharp
namespace MyApp.Pages;

public sealed class MyPage : Page
{
    public MyPage()
    {
        var cardGrid = new ResponsiveCardGrid
        {
            Items = new List<CardItem>
            {
                new("機能 A", "説明", "\uE710"),
            }
        };
        // ... レイアウト構築
    }
}
```

**2. `MainWindow.xaml` に NavigationViewItem を追加**
```xml
<NavigationViewItem Tag="mypage" Content="マイページ">
    <NavigationViewItem.Icon>
        <FontIcon Glyph="&#xE710;"/>
    </NavigationViewItem.Icon>
</NavigationViewItem>
```

**3. `MainWindow.xaml.cs` の switch に遷移先を追加**
```csharp
var (pageType, title) = tag switch
{
    "home"   => (typeof(HomePage), "ホーム"),
    "mypage" => (typeof(MyPage),   "マイページ"),  // ← 追加
    _        => (typeof(HomePage), "ホーム")
};
```

---

## カードグリッドのカスタマイズ
```csharp
CardGrid.Items = new List<CardItem>
{
    new("タイトル", "説明文", "\uE710"),        // アイコンあり
    new("タイトル", "説明文", "\uE8A5", myObj), // Tag に任意データを渡せる
};

// クリック時の処理
CardGrid.ItemClick = item =>
{
    // item.Title や item.Tag で分岐
};

// カードの目安幅を変更（デフォルト 220px）
CardGrid.IdealItemWidth = 280;
```

Segoe Fluent Icons のグリフ一覧は [Microsoft Docs](https://learn.microsoft.com/ja-jp/windows/apps/design/style/segoe-fluent-icons-font) を参照してください。

---

## 設定項目の追加

`SettingsService.cs` の `SettingsData` にプロパティを追加します。
```csharp
public class SettingsData
{
    // 既存の項目 ...

    public bool   ShowNotifications { get; set; } = true;
    public string LastOpenedFile    { get; set; } = "";
}
```

`SettingsService` にも同名のプロキシプロパティを追加します。
```csharp
public bool   ShowNotifications { get => _data.ShowNotifications; set => _data.ShowNotifications = value; }
public string LastOpenedFile    { get => _data.LastOpenedFile;    set => _data.LastOpenedFile = value; }
```

---

## ビルドと実行
```bash
# Visual Studio から実行
F5（デバッグ実行）

# コマンドラインでビルド
dotnet build -c Release

# 出力先
bin/x64/Release/net8.0-windows10.0.19041.0/
```

アンパッケージド実行のため MSIX は不要です。`exe` をそのまま配布できます。

---

## ライセンス

MIT
