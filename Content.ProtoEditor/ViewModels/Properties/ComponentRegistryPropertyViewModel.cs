using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.ViewModels.Properties;

[ViewModelFor(typeof(ComponentRegistry))]
[UsedImplicitly]
public sealed partial class ComponentRegistryPropertyViewModel : PropertyViewModel
{
    private ComponentRegistry _value;

    [ObservableProperty]
    private int _numComponents;

    public ComponentRegistryPropertyViewModel(MemberInfo info, object? value) : base(info)
    {
        _value = (value as ComponentRegistry)!;
        _numComponents = _value.Count;
    }

    [RelayCommand]
    private void ShowDetails()
    {
    }

    public override void SaveToInstance(IPrototype instance)
    {
        // TODO: Implement
    }
}
