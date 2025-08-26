using Avalonia;
using Avalonia.Controls;
using Content.ProtoEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Content.ProtoEditor.Views;

/// <summary>
/// View for showing all prototypes in a List, for browsing, filtering, and selecting.
/// </summary>
public sealed partial class PrototypeListView : UserControl
{
    public PrototypeListView()
    {
        InitializeComponent();

        var app = Application.Current as App;
        DataContext = app!.Services.GetRequiredService<PrototypeListViewModel>();
    }
}
