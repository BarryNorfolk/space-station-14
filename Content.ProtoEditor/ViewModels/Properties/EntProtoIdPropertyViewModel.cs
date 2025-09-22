using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Content.ProtoEditor.Messages;
using JetBrains.Annotations;
using ReactiveUI;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.ViewModels.Properties;

[ViewModelFor(typeof(EntProtoId))]
[UsedImplicitly]
public sealed partial class EntProtoIdPropertyViewModel : PropertyViewModel
{
    private EntProtoId? _actual = null;

    [ObservableProperty]
    private string _protoId = "null";

    [ObservableProperty]
    private bool _isValidProtoEnt = false;

    public EntProtoIdPropertyViewModel(MemberInfo info, EntProtoId? proto) : base(info)
    {
        if (!proto.HasValue)
            return;

        var value = proto.Value;
        _actual = proto;
        _protoId = value.ToString();
        _isValidProtoEnt = true;
    }

    [RelayCommand]
    private void GotoId()
    {
        MessageBus.Current.SendMessage(new PrototypeIdSelectedMessage(ProtoId));
    }

    [RelayCommand]
    private void SetId()
    {
        // TODO: Implement this so it opens a whole window for selection
    }

    public override void SaveToInstance(IPrototype instance)
    {
        // TODO: Make it save
    }
}
