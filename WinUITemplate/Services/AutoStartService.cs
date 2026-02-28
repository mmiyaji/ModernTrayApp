using Microsoft.Win32;
using System;
using System.Reflection;

namespace WinUITemplate.Services;

/// <summary>
/// Windows のスタートアップへの登録を管理するサービス。
/// HKCU\Software\Microsoft\Windows\CurrentVersion\Run を使用。
/// </summary>
public static class AutoStartService
{
    private const string RegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "WinUITemplate";

    public static void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, writable: true);
            if (key is null) return;

            if (enable)
            {
                var exePath = Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location;
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
            }
        }
        catch
        {
            // レジストリへのアクセス失敗は無視
        }
    }

    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey);
            return key?.GetValue(AppName) is not null;
        }
        catch
        {
            return false;
        }
    }
}
