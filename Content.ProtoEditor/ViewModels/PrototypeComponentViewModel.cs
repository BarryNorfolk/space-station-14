using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Content.ProtoEditor.Messages;
using Content.ProtoEditor.Services;
using ReactiveUI;
using Robust.Shared.Utility;

namespace Content.ProtoEditor.ViewModels;

public sealed partial class PrototypeComponentViewModel : ViewModelBase
{
    /// <summary>
    /// Stored reference to the PrototypeManager provided by the ProtoEditor.
    /// </summary>
    private readonly PrototypeProvider _prototypeProvider;

    [ObservableProperty]
    private string _selectedPrototype = "none";

    [ObservableProperty]
    private string _infoText = "";

    /// <summary>
    /// Task for handling Initialization of this model view.
    /// Can be await'ed on by other dependents to ensure required properties are setup.
    /// </summary>
    public Task Initialization { get; private set; }

    public PrototypeComponentViewModel(PrototypeProvider prototypeProvider)
    {
        _prototypeProvider = prototypeProvider;

        MessageBus.Current.Listen<PrototypeSelectedMessage>()
            .Subscribe(x => OnPrototypeSelected(x.Prototype));

        Initialization = InitializeAsync();
    }

    private void OnPrototypeSelected(PrototypeViewModel? prototype)
    {
        InfoText = "";
        if (prototype == null)
            return;

        /*
            Show all the properties of the particular thing.
            TODO:
                - Visualisers for each available "Type" that we know about
                    I.e. String shows text box, boolean shows checkbox, etc.
                - Learn how to use DataField versus the actual InComponent name.
                    I.e. Not sure we want to show "SetName" when it's actually "Name".
        */

        SelectedPrototype = prototype.Id;
        InfoText = "-- Properties --\n";
        foreach (var property in prototype.Kind.GetAllProperties())
        {
            if (!property.IsBasePropertyDefinition())
            {
                continue;
            }

            InfoText += $"  ({PrettyPrint.PrintUserFacingTypeShort(property.PropertyType, 2)}) {property.Name} => {property.GetValue(prototype.Instance)}\n";
        }
    }

    /// <summary>
    /// Async Initialization of this ViewModel.
    /// </summary>
    /// <returns>The async task for the initialization.</returns>
    private async Task InitializeAsync()
    {
        await _prototypeProvider.Initialization;
    }
}
