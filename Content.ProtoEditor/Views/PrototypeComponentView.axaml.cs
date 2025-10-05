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
public sealed partial class PrototypeComponentView : UserControl
{
    public PrototypeComponentView()
    {
        if (Design.IsDesignMode)
        {
            var prototypes = new DesignPrototypeProvider();
            var dummy = prototypes.GetPrototypeModel("DummyPrototype");
            var vm = new PrototypeComponentViewModel(prototypes, new PropertyViewModelFactory());
            vm.SelectPrototype(dummy.Value);
            Design.SetDataContext(this, vm);
        }
        else
        {
            var app = Application.Current as App;
            DataContext = app!.Services.GetRequiredService<PrototypeComponentViewModel>();
        }

        InitializeComponent();
    }
}
