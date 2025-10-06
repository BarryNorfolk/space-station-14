using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Content.ProtoEditor.Messages;
using Content.ProtoEditor.Services;
using ReactiveUI;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.ViewModels;

public sealed partial class PrototypeListViewModel : ViewModelBase
{
    /// <summary>
    /// Stored reference to the PrototypeManager provided by the ProtoEditor.
    /// </summary>
    private readonly IPrototypeProvider _prototypeProvider;

    /// <summary>
    /// String to use for filtering prototypes in the list view.
    /// </summary>
    [ObservableProperty]
    private string _listFilterText = "";

    /// <summary>
    /// The currently selected prototype, if any, in the list of available ones.
    /// </summary>
    [ObservableProperty]
    private PrototypeViewModel? _selectedPrototype;

    public PrototypeListViewModel(IPrototypeProvider prototypeProvider)
    {
        _prototypeProvider = prototypeProvider;

        MessageBus.Current.Listen<PrototypeIdSelectedMessage>()
            .Subscribe(x => OnPrototypeIdSelected(x.ProtoId));

        Initialization = InitializeAsync();
    }

    /// <summary>
    /// List of all known "Kinds" of prototypes, updated as the prototype manager reloads and
    /// can be used for filtering.
    /// </summary>
    public ObservableCollection<PrototypeKindViewModel> Kinds { get; set; } = [];

    /// <summary>
    /// Task for handling Initialization of this model view.
    /// Can be await'ed on by other dependents to ensure required properties are setup.
    /// </summary>
    public Task Initialization { get; private set; }

    /// <summary>
    /// Handles when the ListFilterText changes and causes a re-filtering of the visible list
    /// of prototypes based on the "ID".
    /// </summary>
    /// <param name="value">New value of the filter text.</param>
    partial void OnListFilterTextChanged(string value)
    {
        foreach (var kind in Kinds)
        {
            kind.FilterSubject.OnNext(FilterPrototypes);
        }
    }

    /// <summary>
    /// Handles when the SelectedItem for the possible PrototypeViewModels changes
    /// and causes an update of the Component view.
    /// </summary>
    /// <param name="value">Selected item in the list of Prototypes.</param>
    partial void OnSelectedPrototypeChanged(PrototypeViewModel? value)
    {
        MessageBus.Current.SendMessage(new PrototypeSelectedMessage(value));
    }

    /// <summary>
    /// Handles when a user control requests us to show a particular prototype.
    /// </summary>
    /// <param name="protoId">The prototype ID to show.</param>
    private void OnPrototypeIdSelected(string protoId)
    {
        var prototype = _prototypeProvider.GetPrototypeModel(protoId);
        if (!prototype.HasValue)
            return; // Failed to find anything

        MessageBus.Current.SendMessage(new PrototypeSelectedMessage(prototype.Value));
    }

    /// <summary>
    /// Handles filtering prototypes based on their kind and the current filtered text.
    /// </summary>
    /// <param name="prototype">The view model of the prototype to be filtered.</param>
    /// <returns>True if the prototype should be visible, otherwise false.</returns>
    private bool FilterPrototypes(PrototypeViewModel prototype)
    {
        // Then perform a string check against the ID of the prototype
        // TODO: Possibly improve fuzzy finding and string validation
        if (!string.IsNullOrWhiteSpace(ListFilterText) && !prototype.Id.Contains(ListFilterText))
            return false;

        return true;
    }

    /// <summary>
    /// Async Initialization of this ViewModel.
    /// </summary>
    /// <returns>The async task for the initialization.</returns>
    private async Task InitializeAsync()
    {
        await _prototypeProvider.Initialization;

        // Reload the kinds, while also making sure All is first
        Kinds.Clear();
        foreach (var kind in _prototypeProvider.GetKinds())
        {
            Kinds.Add(kind);
        }
    }

    /// <summary>
    /// Janky way of having a special "All" prototype that can be listed as a Kind
    /// on the Selection combobox
    /// TODO: Maybe this should be removed in favour of always finding/selecting
    /// "Robust.Shared.Prototypes.EntityPrototype" by default.
    /// </summary>
    private sealed class All : IPrototype
    {
        public string ID { get; } = "Dummy";
    }
}
