# WinUI 3 アプリテンプレート

WinUI 3 (Windows App SDK) を使ったアプリを量産するためのテンプレートプロジェクトです。

## ✨ 機能

| 機能 | 詳細 |
|------|------|
| NavigationView | 左ナビゲーション + 設定ページ |
| テーマ切り替え | ライト / ダーク / システムに合わせる |
| タスクトレイ常駐 | WinForms NotifyIcon を利用 |
| 最小化でトレイに格納 | ×ボタンでトレイに格納（設定で切替可） |
| 自動起動 | レジストリ (HKCU\...\Run) に登録 |
| 設定の永続化 | JSON ファイルで保存（LocalAppData） |

## 📁 プロジェクト構成

```
WinUITemplate/
├── WinUITemplate.sln
└── WinUITemplate/
    ├── App.xaml / App.xaml.cs         ← アプリエントリ、トレイ・テーマ初期化
    ├── MainWindow.xaml / .cs          ← メインウィンドウ、ナビゲーション
    ├── app.manifest                   ← DPI 設定
    ├── Pages/
    │   ├── HomePage.xaml / .cs        ← ホーム（カスタマイズ起点）
    │   └── SettingsPage.xaml / .cs    ← 設定画面
    ├── Services/
    │   ├── SettingsService.cs         ← 設定の読み書き (JSON)
    │   ├── TrayIconService.cs         ← タスクトレイアイコン
    │   └── AutoStartService.cs        ← Windows 自動起動
    ├── Helpers/
    │   └── WindowHelper.cs            ← Win32 ウィンドウ操作
    └── Styles/
        └── AppStyles.xaml             ← カードスタイルなど共通スタイル
```

## 🚀 セットアップ

### 必要環境
- Visual Studio 2022 以降
- Windows Application Development ワークロード
- Windows 10 (1903) 以降

### ビルド手順
1. `WinUITemplate.sln` を Visual Studio で開く
2. NuGet パッケージを復元
3. `WinUITemplate` をスタートアッププロジェクトに設定してビルド

## 🔧 新しいアプリを作るとき

1. このテンプレートをコピーしてフォルダごとリネーム
2. `.sln` / `.csproj` 内の `WinUITemplate` を新しいアプリ名に一括置換
3. `RootNamespace` を変更
4. `HomePage.xaml` を編集してメインコンテンツを実装
5. 必要に応じて `NavView.MenuItems` にページを追加

## 📦 依存パッケージ

- `Microsoft.WindowsAppSDK` 1.5.x
- `CommunityToolkit.WinUI.UI` 8.1.x
- WinForms (UseWindowsForms=true でトレイアイコン実現)

## ⚠️ 注意事項

- タスクトレイ機能は WinForms の `NotifyIcon` に依存しています（WinUI 3 はネイティブトレイ API 非対応のため）
- MSIX パッケージ化する場合、自動起動は `Windows.ApplicationModel.StartupTask` API での実装を推奨します
