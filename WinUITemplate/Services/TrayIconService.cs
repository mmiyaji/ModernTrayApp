using System;
using System.Runtime.InteropServices;

namespace WinUITemplate.Services;

/// <summary>
/// Win32 Shell_NotifyIcon を直接呼び出してタスクトレイアイコンを管理するサービス。
/// WinForms / WPF への依存なし。
/// </summary>
public class TrayIconService : IDisposable
{
    // -------- Win32 定義 --------
    private const int WM_APP = 0x8000;
    private const int TRAY_CALLBACK = WM_APP + 1;
    private const int WM_LBUTTONDBLCLK = 0x0203;
    private const int WM_RBUTTONUP = 0x0205;
    private const int NIM_ADD = 0x00000000;
    private const int NIM_MODIFY = 0x00000001;
    private const int NIM_DELETE = 0x00000002;
    private const int NIF_MESSAGE = 0x00000001;
    private const int NIF_ICON = 0x00000002;
    private const int NIF_TIP = 0x00000004;
    private const int NIF_INFO = 0x00000010;
    private const int NIIF_INFO = 0x00000001;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public int cbSize;
        public nint hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public nint hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;
        public int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public int uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll")]
    private static extern nint LoadIcon(nint hInstance, nint lpIconName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint CreateWindowEx(int exStyle, string className, string windowName,
        int style, int x, int y, int width, int height,
        nint parent, nint menu, nint instance, nint param);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(nint hWnd);

    [DllImport("user32.dll")]
    private static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll")]
    private static extern nint CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenu(nint hMenu, uint uFlags, nint uIDNewItem, string lpNewItem);

    [DllImport("user32.dll")]
    private static extern int TrackPopupMenu(nint hMenu, uint uFlags, int x, int y,
        int nReserved, nint hWnd, nint prcRect);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(nint hMenu);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int x, y; }

    private delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public int cbSize, style;
        public nint lpfnWndProc;
        public int cbClsExtra, cbWndExtra;
        public nint hInstance, hIcon, hCursor, hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
        public nint hIconSm;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern short RegisterClassEx(ref WNDCLASSEX lpwcx);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool UnregisterClass(string lpClassName, nint hInstance);

    [DllImport("kernel32.dll")]
    private static extern nint GetModuleHandle(string? lpModuleName);
    // -------- ここまで Win32 --------

    private const int CMD_SHOW = 1001;
    private const int CMD_EXIT = 1002;
    private const uint MF_STRING = 0x00000000;
    private const uint MF_SEPARATOR = 0x00000800;
    private const uint TPM_RIGHTBUTTON = 0x0002;

    public event Action? ShowWindowRequested;
    public event Action? ExitRequested;

    private nint _hwnd;
    private NOTIFYICONDATA _nid;
    private WndProcDelegate? _wndProcDelegate; // GC対策で保持
    private bool _visible;
    private bool _disposed;
    private readonly string _className = $"WinUITrayWnd_{Guid.NewGuid():N}";

    public TrayIconService()
    {
        CreateMessageWindow();
    }

    private void CreateMessageWindow()
    {
        _wndProcDelegate = WndProc;
        var hInstance = GetModuleHandle(null);

        var wc = new WNDCLASSEX
        {
            cbSize = Marshal.SizeOf<WNDCLASSEX>(),
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = hInstance,
            lpszClassName = _className
        };
        RegisterClassEx(ref wc);

        // HWND_MESSAGE ウィンドウ（不可視）
        _hwnd = CreateWindowEx(0, _className, "TrayHost", 0, 0, 0, 0, 0,
            new nint(-3), nint.Zero, hInstance, nint.Zero);

        _nid = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = TRAY_CALLBACK,
            hIcon = LoadIcon(nint.Zero, new nint(32512)), // IDI_APPLICATION
            szTip = "WinUI Template App"
        };
    }

    private nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == TRAY_CALLBACK)
        {
            int mouseMsg = (int)(lParam & 0xFFFF);
            if (mouseMsg == WM_LBUTTONDBLCLK)
            {
                ShowWindowRequested?.Invoke();
            }
            else if (mouseMsg == WM_RBUTTONUP)
            {
                ShowContextMenu();
            }
        }
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void ShowContextMenu()
    {
        var hMenu = CreatePopupMenu();
        AppendMenu(hMenu, MF_STRING, CMD_SHOW, "ウィンドウを表示");
        AppendMenu(hMenu, MF_SEPARATOR, nint.Zero, "");
        AppendMenu(hMenu, MF_STRING, CMD_EXIT, "終了");

        SetForegroundWindow(_hwnd);
        GetCursorPos(out var pt);
        int cmd = TrackPopupMenu(hMenu, TPM_RIGHTBUTTON, pt.x, pt.y, 0, _hwnd, nint.Zero);
        DestroyMenu(hMenu);

        if (cmd == CMD_SHOW) ShowWindowRequested?.Invoke();
        else if (cmd == CMD_EXIT) ExitRequested?.Invoke();
    }

    public void Show()
    {
        if (_visible) return;
        Shell_NotifyIcon(NIM_ADD, ref _nid);
        _visible = true;
    }

    public void Hide()
    {
        if (!_visible) return;
        Shell_NotifyIcon(NIM_DELETE, ref _nid);
        _visible = false;
    }

    public void ShowBalloonTip(string title, string message)
    {
        if (!_visible) return;
        var nid = _nid;
        nid.uFlags |= NIF_INFO;
        nid.szInfoTitle = title;
        nid.szInfo = message;
        nid.dwInfoFlags = NIIF_INFO;
        nid.uTimeoutOrVersion = 3000;
        Shell_NotifyIcon(NIM_MODIFY, ref nid);
    }

    public void Dispose()
    {
        if (_disposed) return;
        Hide();
        if (_hwnd != nint.Zero) DestroyWindow(_hwnd);
        UnregisterClass(_className, GetModuleHandle(null));
        _disposed = true;
    }
}