using Avalonia;
using Avalonia.Controls;
using Content.ProtoEditor.Designer;
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
        if (Design.IsDesignMode)
        {
            var prototypes = new DesignPrototypeProvider();
            var vm = new PrototypeListViewModel(prototypes);
            Design.SetDataContext(this, vm);
        }
        else
        {
            var app = Application.Current as App;
            DataContext = app!.Services.GetRequiredService<PrototypeListViewModel>();
        }

        InitializeComponent();
    }
}
