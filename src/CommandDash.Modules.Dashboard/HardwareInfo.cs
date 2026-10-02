using Microsoft.Win32;

namespace CommandDash.Modules.Dashboard;

/// <summary>Hardware model names, detected once on startup.</summary>
internal static class HardwareInfo
{
    private const string GpuClassKey = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";

    public static string? CpuName { get; } = DetectCpu();

    public static string? GpuName { get; } = DetectGpu();

    private static string? DetectCpu()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return Clean(key?.GetValue("ProcessorNameString") as string);
        }
        catch
        {
            return null;
        }
    }

    private static string? DetectGpu()
    {
        try
        {
            using var classKey = Registry.LocalMachine.OpenSubKey(GpuClassKey);
            if (classKey is null) return null;

            var names = new List<string>();
            foreach (var sub in classKey.GetSubKeyNames().Where(n => n.All(char.IsDigit)))
            {
                using var adapter = classKey.OpenSubKey(sub);
                var name = Clean(adapter?.GetValue("DriverDesc") as string);
                if (name is null) continue;
                if (name.Contains("Microsoft Basic", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("Remote Display", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("Virtual", StringComparison.OrdinalIgnoreCase)) continue;
                names.Add(name);
            }

            // Prefer a discrete GPU over integrated graphics when both exist.
            return names.FirstOrDefault(n => n.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)
                                             || n.Contains("Radeon RX", StringComparison.OrdinalIgnoreCase)
                                             || n.Contains("Arc", StringComparison.OrdinalIgnoreCase))
                   ?? names.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static string? Clean(string? value)
    {
        value = value?.Trim();
        return string.IsNullOrEmpty(value) ? null : System.Text.RegularExpressions.Regex.Replace(value, @"\s+", " ");
    }
}
