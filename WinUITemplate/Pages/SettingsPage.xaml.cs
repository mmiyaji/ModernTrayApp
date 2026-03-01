using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUITemplate.Services;

namespace WinUITemplate.Pages;

public sealed partial class SettingsPage : Page
{
    private bool _isInitializing = true;

    public SettingsPage()
    {
        this.InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        _isInitializing = true;

        ThemeComboBox.SelectedIndex = App.Settings.AppTheme switch
        {
            AppTheme.Light => 1,
            AppTheme.Dark => 2,
            _ => 0
        };
        OpacitySlider.Value = App.Settings.WindowOpacity;
        AutoStartToggle.IsOn = App.Settings.AutoStart;
        TrayIconToggle.IsOn = App.Settings.EnableTrayIcon;
        MinimizeToTrayToggle.IsOn = App.Settings.MinimizeToTray;

        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"バージョン {version?.Major}.{version?.Minor}.{version?.Build}";

        _isInitializing = false;
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        var theme = ThemeComboBox.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.System
        };
        App.Settings.AppTheme = theme;
        App.Settings.Save();
        App.ApplyTheme(theme);
    }

    private void OpacitySlider_ValueChanged(object sender,
        Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_isInitializing) return;
        App.Settings.WindowOpacity = (int)OpacitySlider.Value;
        App.Settings.Save();
    }

    private void AutoStartToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        App.Settings.AutoStart = AutoStartToggle.IsOn;
        App.Settings.Save();
        AutoStartService.SetAutoStart(AutoStartToggle.IsOn);
    }

    private void TrayIconToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        App.Settings.EnableTrayIcon = TrayIconToggle.IsOn;
        App.Settings.Save();
        if (TrayIconToggle.IsOn) App.TrayIcon?.Show();
        else App.TrayIcon?.Hide();
    }

    private void MinimizeToTrayToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        App.Settings.MinimizeToTray = MinimizeToTrayToggle.IsOn;
        App.Settings.Save();
    }
}