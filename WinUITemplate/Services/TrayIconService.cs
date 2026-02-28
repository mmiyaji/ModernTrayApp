using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WinUITemplate.Services;

public class TrayIconService : IDisposable
{
    // Win32
    private const int WM_APP = 0x8000;
    private const int TRAY_CALLBACK = WM_APP + 1;
    private const int WM_LBUTTONDBLCLK = 0x0203;
    private const int WM_RBUTTONUP = 0x0205;
    private const int NIM_ADD = 0, NIM_MODIFY = 1, NIM_DELETE = 2;
    private const int NIF_MESSAGE = 1, NIF_ICON = 2, NIF_TIP = 4, NIF_INFO = 16;
    private const int NIIF_INFO = 1;
    private const uint IMAGE_ICON = 1;
    private const uint LR_LOADFROMFILE = 0x10;
    private const uint LR_DEFAULTSIZE = 0x40;
    private const uint MF_STRING = 0, MF_SEPARATOR = 0x800;
    private const uint TPM_RIGHTBUTTON = 2;
    private const int CMD_SHOW = 1001, CMD_EXIT = 1002;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public int cbSize;
        public nint hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public nint hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string szTip;
        public int dwState, dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string szInfo;
        public int uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string szInfoTitle;
        public int dwInfoFlags;
    }

    [StructLayout(LayoutKind.Sequential)] private struct POINT { public int x, y; }

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

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)] private static extern bool Shell_NotifyIcon(int msg, ref NOTIFYICONDATA data);
    [DllImport("user32.dll")] private static extern nint LoadIcon(nint hInstance, nint name);
    [DllImport("user32.dll")] private static extern nint LoadImage(nint hInst, string name, uint type, int cx, int cy, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern nint CreateWindowEx(int ex, string cls, string name, int style, int x, int y, int w, int h, nint parent, nint menu, nint inst, nint param);
    [DllImport("user32.dll")] private static extern bool DestroyWindow(nint hWnd);
    [DllImport("user32.dll")] private static extern nint DefWindowProc(nint hWnd, uint msg, nint w, nint l);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out POINT pt);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(nint hWnd);
    [DllImport("user32.dll")] private static extern nint CreatePopupMenu();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool AppendMenu(nint hMenu, uint flags, nint id, string text);
    [DllImport("user32.dll")] private static extern int TrackPopupMenu(nint hMenu, uint flags, int x, int y, int reserved, nint hWnd, nint rect);
    [DllImport("user32.dll")] private static extern bool DestroyMenu(nint hMenu);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern short RegisterClassEx(ref WNDCLASSEX wc);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool UnregisterClass(string cls, nint inst);
    [DllImport("kernel32.dll")] private static extern nint GetModuleHandle(string? name);

    private delegate nint WndProcDelegate(nint hWnd, uint msg, nint w, nint l);

    public event Action? ShowWindowRequested;
    public event Action? ExitRequested;

    private nint _hwnd;
    private NOTIFYICONDATA _nid;
    private WndProcDelegate? _wndProc;
    private bool _visible, _disposed;
    private readonly string _className = $"TrayWnd_{Guid.NewGuid():N}";

    public TrayIconService()
    {
        var hInst = GetModuleHandle(null);
        _wndProc = WndProc;

        var wc = new WNDCLASSEX
        {
            cbSize = Marshal.SizeOf<WNDCLASSEX>(),
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc),
            hInstance = hInst,
            lpszClassName = _className
        };
        RegisterClassEx(ref wc);
        _hwnd = CreateWindowEx(0, _className, "", 0, 0, 0, 0, 0, new nint(-3), nint.Zero, hInst, nint.Zero);

        // アイコン読み込み（.ico ファイル優先、なければシステムデフォルト）
        nint hIcon = LoadAppIcon();

        _nid = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = TRAY_CALLBACK,
            hIcon = hIcon,
            szTip = "WinUI Template App"
        };
    }

    private static nint LoadAppIcon()
    {
        var exeDir = Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        var icoPath = Path.Combine(exeDir, "Assets", "AppIcon.ico");

        if (File.Exists(icoPath))
        {
            var hIcon = LoadImage(nint.Zero, icoPath, IMAGE_ICON, 16, 16, LR_LOADFROMFILE);
            if (hIcon != nint.Zero) return hIcon;
        }
        // フォールバック: システムアイコン
        return LoadIcon(nint.Zero, new nint(32512)); // IDI_APPLICATION
    }

    private nint WndProc(nint hWnd, uint msg, nint w, nint l)
    {
        if (msg == TRAY_CALLBACK)
        {
            int m = (int)(l & 0xFFFF);
            if (m == WM_LBUTTONDBLCLK) ShowWindowRequested?.Invoke();
            else if (m == WM_RBUTTONUP) ShowContextMenu();
        }
        return DefWindowProc(hWnd, msg, w, l);
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