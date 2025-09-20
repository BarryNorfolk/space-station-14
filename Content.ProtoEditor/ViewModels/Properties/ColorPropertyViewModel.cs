using System.Reflection;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.ViewModels.Properties;

[ViewModelFor(typeof(Robust.Shared.Maths.Color))]
[UsedImplicitly]
public sealed partial class ColorPropertyViewModel(MemberInfo info, Robust.Shared.Maths.Color value)
    : PropertyViewModel(info)
{
    [ObservableProperty]
    private Color _selectedColor = new(value.AByte, value.RByte, value.GByte, value.BByte);

    public override void SaveToInstance(IPrototype instance)
    {
        Save(instance,
            new Robust.Shared.Maths.Color(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A));
    }
}
