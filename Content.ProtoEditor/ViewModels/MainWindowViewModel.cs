using CommunityToolkit.Mvvm.ComponentModel;

namespace Content.ProtoEditor.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Simple text for showing some information on what the program is doing right now
    /// TODO: This could be a more impactful "State" update system, we should make something
    /// that shows users what is going on. In case there are any long lived processes occuring.
    /// </summary>
    [ObservableProperty]
    private string _state = "Started";
}
