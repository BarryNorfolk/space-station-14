using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Content.ProtoEditor.Services;

namespace Content.ProtoEditor.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Simple text for showing some information on what the program is doing right now
    ///
    /// TODO: This could be a more impactful "State" update system, we should make something
    /// that shows users what is going on. In case there are any long lived processes occuring.
    /// </summary>
    [ObservableProperty]
    private string _state = "Starting up";

    /// <summary>
    /// Stored reference to the PrototypeManager provided by the ProtoEditor.
    /// </summary>
    private readonly DependencyProvider _assemblyProvider;

    /// <summary>
    /// Task for handling Initialization of this model view.
    /// Can be await'ed on by other dependents to ensure required properties are setup.
    /// </summary>
    public Task Initialization { get; private set; }

    public MainWindowViewModel(DependencyProvider assemblyProvider)
    {
        _assemblyProvider = assemblyProvider;

        Initialization = InitializeAsync();
    }

    /// <summary>
    /// Async Initialization of this ViewModel.
    /// </summary>
    /// <returns>The async task for the initialization.</returns>
    private async Task InitializeAsync()
    {
        State = "Assemblies Loaded.";
    }
}
