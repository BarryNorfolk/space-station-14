using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Content.ProtoEditor.Messages;
using Content.ProtoEditor.Services;
using Content.ProtoEditor.ViewModels.Properties;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ReactiveUI;
using Robust.Shared.Utility;

namespace Content.ProtoEditor.ViewModels;

// TODO: Rename this as it's not really a component view
public sealed partial class PrototypeComponentViewModel : ViewModelBase
{
    /// <summary>
    /// Stored reference to the PrototypeManager provided by the ProtoEditor.
    /// </summary>
    private readonly PrototypeProvider _prototypeProvider;

    /// <summary>
    /// Stored reference to the PrototypeManager provided by the ProtoEditor.
    /// </summary>
    private readonly PropertyViewModelFactory _viewModelFactory;

    /// <summary>
    /// Simple list of types to ignore when processing fields and properties of a prototype.
    /// We _could_ be more specific and either use the full name of the type, or do a comparison by
    /// type, but then that means perhaps polluting this file with many imports.
    /// </summary>
    private readonly List<string> _ignoredTypes = [
        "ILocalizationManager"
    ];

    [ObservableProperty]
    private string _selectedName = "none";

    [ObservableProperty]
    private string _parents = "?";

    private PrototypeViewModel? _selectedPrototype = null;

    public ObservableCollection<PropertyViewModel> Properties { get; } = [];

    /// <summary>
    /// Task for handling Initialization of this model view.
    /// Can be await'ed on by other dependents to ensure required properties are setup.
    /// </summary>
    public Task Initialization { get; private set; }

    public PrototypeComponentViewModel(PrototypeProvider prototypeProvider, PropertyViewModelFactory viewModelFactory)
    {
        _prototypeProvider = prototypeProvider;
        _viewModelFactory = viewModelFactory;

        MessageBus.Current.Listen<PrototypeSelectedMessage>()
            .Subscribe(x => OnPrototypeSelected(x.Prototype));

        Initialization = InitializeAsync();
    }

    [RelayCommand]
    private void SaveChanges()
    {
        if (_selectedPrototype == null)
            return;

        foreach (var property in Properties)
        {
            if (property is FallbackPropertyViewModel)
            {
                // Don't attempt to set fields/properties on things we can't render
                continue;
            }

            if (property.IsFrozen)
            {
                // We can't write to this field/property so don't bother.
                continue;
            }

            property.SaveToInstance(_selectedPrototype.Instance);
        }
    }

    private void OnPrototypeSelected(PrototypeViewModel? prototype)
    {
        Properties.Clear();
        if (prototype == null)
        {
            SelectedName = "None";
            _selectedPrototype = null;
            return;
        }

        /*
            Show all the properties of the particular thing.
            TODO:
            - Consider Ignored properties per kind, things like Name which clash with `SetName` (which has a datafield for 'name')
        */

        _selectedPrototype = prototype;
        SelectedName = prototype.Id;
        Parents = _prototypeProvider.GetParents(prototype);
        var baseFields = _prototypeProvider.GetBaseFields(prototype);

        foreach (var property in prototype.Kind.GetAllProperties())
        {
            if (!property.IsBasePropertyDefinition() ||
                _ignoredTypes.Contains(property.PropertyType.Name))
            {
                continue;
            }

            var vm = _viewModelFactory.CreateFromProperty(property, prototype.Instance);
            vm.IsFrozen = !property.CanWrite;

            if (baseFields.TryGetValue(vm.Name, out var data))
            {
                vm.SetInheritedFields(data);
            }

            Properties.Add(vm);
        }

        foreach (var field in prototype.Kind.GetAllFields())
        {
            if (field.IsBackingField() ||
                _ignoredTypes.Contains(field.FieldType.Name))
            {
                /*
                    TODO: Determine if this is correct, do we really want to drop all the backing fields
                    from being shown? I'll have to research a bit more.
                */
                continue;
            }

            var vm = _viewModelFactory.CreateFromField(field, prototype.Instance);

            if (baseFields.TryGetValue(vm.Name, out var data))
            {
                vm.SetInheritedFields(data);
            }

            Properties.Add(vm);
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
