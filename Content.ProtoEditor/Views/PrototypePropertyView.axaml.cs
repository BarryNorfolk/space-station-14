using Avalonia;
using Avalonia.Controls;
using Content.ProtoEditor.Designer;
using Content.ProtoEditor.ViewModels;
using Content.ProtoEditor.ViewModels.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace Content.ProtoEditor.Views;

/// <summary>
/// View for showing the details of a selected prototype, and allow for editing.
/// </summary>
public sealed partial class PrototypePropertyView : UserControl
{
    public PrototypePropertyView()
    {
        if (Design.IsDesignMode)
        {
            var prototypes = new DesignPrototypeProvider();
            var vm = new PrototypePropertyViewModel(prototypes, new PropertyViewModelFactory());
            vm.SelectPrototype(prototypes.GetPrototypeModel("Components").Value);
            Design.SetDataContext(this, vm);
        }
        else
        {
            var app = Application.Current as App;
            DataContext = app!.Services.GetRequiredService<PrototypePropertyViewModel>();
        }

        InitializeComponent();
    }
}
