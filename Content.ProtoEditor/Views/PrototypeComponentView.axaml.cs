using Avalonia;
using Avalonia.Controls;
using Content.ProtoEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Content.ProtoEditor.Views;

/// <summary>
/// View for showing the details of a selected prototype, and allow for editing.
/// </summary>
public sealed partial class PrototypeComponentView : UserControl
{
    public PrototypeComponentView()
    {
        InitializeComponent();

        var app = Application.Current as App;
        DataContext = app!.Services.GetRequiredService<PrototypeComponentViewModel>();
    }
}
