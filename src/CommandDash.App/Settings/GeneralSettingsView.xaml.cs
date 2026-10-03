using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace CommandDash.App.Settings;

public partial class GeneralSettingsView : UserControl
{
	public GeneralSettingsView()
	{
		InitializeComponent();
		RefreshHotkeyText();
		using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath))
		{
			StartupCheckBox.IsChecked = key?.GetValue(RunValueName) is not null;
		}
	}

	private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
	private const string RunValueName = "CommandDash";

	private void Startup_Changed(object sender, RoutedEventArgs e)
	{
		using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
		if (StartupCheckBox.IsChecked == true)
		{
			if (Environment.ProcessPath is { } path)
				key.SetValue(RunValueName, $"\"{path}\"");
		}
		else
		{
			key.DeleteValue(RunValueName, false);
		}
	}

	private void OpenAppFolder_Click(object sender, RoutedEventArgs e)
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = AppContext.BaseDirectory,
			UseShellExecute = true,
		});
	}

	private void RefreshHotkeyText()
	{
		HotkeyText.Text = GeneralSettings.Load().GetHotkey()?.ToString() ?? "(none)";
	}

	private void ChangeHotkey_Click(object sender, RoutedEventArgs e)
	{
		var settings = GeneralSettings.Load();
		var window = new HotkeyBindWindow(Window.GetWindow(this), settings.GetHotkey());
		window.ShowDialog();
		switch (window.Outcome)
		{
			case BindOutcome.Ok when window.Result is { } hotkey:
				settings.Hotkey = hotkey.ToString();
				break;
			case BindOutcome.Clear:
				settings.Hotkey = string.Empty;
				break;
			default:
				return;
		}

		settings.Save();
		RefreshHotkeyText();
	}

	private void DefaultHotkey_Click(object sender, RoutedEventArgs e)
	{
		var settings = GeneralSettings.Load();
		settings.Hotkey = AppHotkey.Default.ToString();
		settings.Save();
		RefreshHotkeyText();
	}
}
