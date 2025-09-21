using System.Reflection;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.ViewModels.Properties;

[ViewModelFor(typeof(Robust.Shared.Maths.Color))]
[UsedImplicitly]
public sealed partial class ColorPropertyViewModel : PropertyViewModel
{
    [ObservableProperty]
    private Color? _selectedColor = null;

    public ColorPropertyViewModel(MemberInfo info, Robust.Shared.Maths.Color? color) : base(info)
    {
        if (!color.HasValue)
            return;

        var value = color.Value;
        _selectedColor = new Color(value.AByte, value.RByte, value.GByte, value.BByte);
    }

    public override void SaveToInstance(IPrototype instance)
    {
        if (!SelectedColor.HasValue)
        {
            if (!IsNullable)
                return; // TODO: Error out here

            Save(instance, null);
            return;
        }

        var value = SelectedColor.Value;
        Save(instance,
            new Robust.Shared.Maths.Color(value.R, value.G, value.B, value.A));
    }
}
