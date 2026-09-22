using CommandDash.Core;

namespace CommandDash.Modules.Sample;

/// <summary>
/// <see cref="IModulePage"/> wrapper around the sample module's WPF view.
/// </summary>
public sealed class SampleModulePage : IModulePage
{
    private readonly SampleModuleView _view = new();

    public object View => _view;

    public void OnNavigatedTo()
    {
        _view.OnShown();
    }

    public void OnNavigatedFrom()
    {
        _view.OnHidden();
    }
}
