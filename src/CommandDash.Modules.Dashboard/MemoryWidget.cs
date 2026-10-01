using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace CommandDash.Modules.Dashboard;

public sealed class MemoryWidget : HardwareWidgetBase
{
    private TextBlock? _value;
    private TextBlock? _detail;
    private ProgressBar? _bar;
    private double _percent;
    private double _usedGb;
    private double _totalGb;

    public override string Id => "commanddash.home.memory";

    public override WidgetGridPosition? DefaultPosition => new(2, 0);

    public override string DisplayName => "Memory";

    public override object CreateView()
    {
        _value = CreateText(32, "TextPrimaryBrush", FontWeights.SemiBold);
        _value.Text = "--%";
        _detail = CreateText(14, "TextSecondaryBrush");
        _bar = CreateBar();
        return CreatePanel(_value, _detail, _bar);
    }

    protected override void Sample()
    {
        var status = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status)) return;

        const double gb = 1024.0 * 1024 * 1024;
        _totalGb = status.TotalPhys / gb;
        _usedGb = (status.TotalPhys - status.AvailPhys) / gb;
        _percent = status.TotalPhys == 0 ? 0 : _usedGb / _totalGb * 100;
    }

    protected override void UpdateView()
    {
        if (_value is not null) _value.Text = $"{_percent:0.0}%";
        if (_detail is not null) _detail.Text = $"{_usedGb:0.0} GB / {_totalGb:0.0} GB";
        if (_bar is not null) _bar.Value = _percent;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);
}
