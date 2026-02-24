using System.Windows;
using System.Windows.Forms; // Windows FormsのNotifyIconを使用

namespace ModernTrayApp.Core
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon? _notifyIcon;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // NotifyIconの初期化
            _notifyIcon = new NotifyIcon();
            // Assetsフォルダからアイコンを読み込み（実行ファイルと同じ場所を想定）
            _notifyIcon.Icon = new System.Drawing.Icon("Assets/tray.ico");
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Modern Tray App";

            // メニュー作成
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("設定を開く", null, (s, a) => ShowMainWindow());
            contextMenu.Items.Add("終了", null, (s, a) => Shutdown());
            _notifyIcon.ContextMenuStrip = contextMenu;

            _notifyIcon.DoubleClick += (s, a) => ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                _mainWindow.Closed += (s, args) => _mainWindow = null;
            }
            _mainWindow.Show();
            _mainWindow.Activate();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}