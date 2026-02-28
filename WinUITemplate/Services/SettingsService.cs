using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinUITemplate.Services;

public enum AppTheme { System, Light, Dark }

public class SettingsData
{
    public AppTheme AppTheme { get; set; } = AppTheme.System;
    public int WindowOpacity { get; set; } = 100;
    public bool AutoStart { get; set; } = false;
    public bool EnableTrayIcon { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;
}

public class SettingsService
{
    private static readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinUITemplate", "settings.json");

    private SettingsData _data = new();

    public AppTheme AppTheme
    {
        get => _data.AppTheme;
        set => _data.AppTheme = value;
    }
    public int WindowOpacity
    {
        get => _data.WindowOpacity;
        set => _data.WindowOpacity = value;
    }
    public bool AutoStart
    {
        get => _data.AutoStart;
        set => _data.AutoStart = value;
    }
    public bool EnableTrayIcon
    {
        get => _data.EnableTrayIcon;
        set => _data.EnableTrayIcon = value;
    }
    public bool MinimizeToTray
    {
        get => _data.MinimizeToTray;
        set => _data.MinimizeToTray = value;
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _data = JsonSerializer.Deserialize<SettingsData>(json) ?? new();
            }
        }
        catch
        {
            _data = new();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch { /* 保存失敗は無視 */ }
    }
}
