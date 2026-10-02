using System.IO;
using System.Text.Json;

namespace CommandDash.App.Settings;

/// <summary>
/// App-wide general settings stored as JSON under %AppData%\CommandDash\general.json.
/// </summary>
public sealed class GeneralSettings
{
	private static readonly string FilePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"CommandDash",
		"general.json");

	/// <summary>Focus hotkey text such as "Alt+`". Empty means no hotkey.</summary>
	public string Hotkey { get; set; } = AppHotkey.Default.ToString();

	public static event EventHandler? Changed;

	public AppHotkey? GetHotkey() => AppHotkey.TryParse(Hotkey, out var hotkey) ? hotkey : null;

	public static GeneralSettings Load()
	{
		try
		{
			if (File.Exists(FilePath))
			{
				return JsonSerializer.Deserialize<GeneralSettings>(File.ReadAllText(FilePath)) ?? new GeneralSettings();
			}
		}
		catch (Exception)
		{
			// Corrupt or unreadable settings fall back to defaults.
		}

		return new GeneralSettings();
	}

	public void Save()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
		File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
		Changed?.Invoke(null, EventArgs.Empty);
	}
}
