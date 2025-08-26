using System;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Content.ProtoEditor.Messages;
using Content.ProtoEditor.Models;
using Content.ProtoEditor.Services;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.ViewModels;

public sealed partial class PrototypeListViewModel : ViewModelBase
{
    /// <summary>
    /// Janky way of having a special "All" prototype that can be listed as a Kind
    /// on the Selection combobox
    ///
    /// TODO: Maybe this should be removed in favour of always finding/selecting
    /// "Robust.Shared.Prototypes.EntityPrototype" by default.
    /// </summary>
    private sealed class All : IPrototype
    {
        public string ID { get; } = "Dummy";
    }

    /// <summary>
    /// String to use for filtering prototypes in the list view.
    /// </summary>
    [ObservableProperty]
    private string _listFilterText = "";

    /// <summary>
    /// Used to enable dynamic filtering of the source prototype list.
    /// </summary>
    private readonly BehaviorSubject<Func<PrototypeViewModel, bool>> _filterSubject =
    new(p => true);

    /// <summary>
    /// Stored reference to the PrototypeManager provided by the ProtoEditor.
    /// </summary>
    private readonly PrototypeProvider _prototypeProvider;

    /// <summary>
    /// Collection that has been filtered/sorted and connected to the list of available prototypes
    /// in the PrototypeManager.
    /// </summary>
    private readonly ReadOnlyObservableCollection<PrototypeViewModel> _prototypeViewModels;
    public ReadOnlyObservableCollection<PrototypeViewModel> PrototypeViewModels => _prototypeViewModels;

    /// <summary>
    /// List of all known "Kinds" of prototypes, updated as the prototype manager reloads and
    /// can be used for filtering.
    /// </summary>
    public ObservableCollection<PrototypeKind> Kinds { get; set; } = [];

    /// <summary>
    /// The 'Kind' to use for filtering prototypes in the list view
    /// </summary>
    [ObservableProperty]
    private PrototypeKind _listFilterKind;

    /// <summary>
    /// Special dummy type for putting at the top of the kind selection combo box,
    /// so that people can filter on "All"
    /// TODO: Better naming of the actual type
    /// </summary>
    private readonly PrototypeKind _defaultAll = new(typeof(All));

    /// <summary>
    /// The currently selected prototype, if any, in the list of available ones.
    /// </summary>
    [ObservableProperty]
    private PrototypeViewModel? _selectedPrototype = null;

    /// <summary>
    /// Task for handling Initialization of this model view.
    /// Can be await'ed on by other dependents to ensure required properties are setup.
    /// </summary>
    public Task Initialization { get; private set; }

    public PrototypeListViewModel(PrototypeProvider prototypeProvider)
    {
        _prototypeProvider = prototypeProvider;

        // This isn't populated yet but we can still bind to the "empty" cache,
        // which will be loaded later on once the async initialization is complete.
        _prototypeProvider.GetPrototypeModels().Connect()
            .Sort(SortExpressionComparer<PrototypeViewModel>.Descending(t => t.Id)) //TODO: Make this actually work?
            .Filter(_filterSubject)
            .Bind(out _prototypeViewModels)
            .Subscribe();

        _listFilterKind = _defaultAll;

        Initialization = InitializeAsync();
    }

    /// <summary>
    /// Handles when the ListFilterText changes and causes a re-filtering of the visible list
    /// of prototypes based on the "ID".
    /// </summary>
    /// <param name="value">New value of the filter text.</param>
    partial void OnListFilterTextChanged(string value)
    {
        _filterSubject.OnNext(FilterPrototypes);
    }

    /// <summary>
    /// Handles when the SelectedItem for the possible PrototypeKinds changes and
    /// causes a re-filtering of the visible list of prototypes.
    /// </summary>
    /// <param name="value">Selected index of the combobox.</param>
    partial void OnListFilterKindChanged(PrototypeKind value)
    {
        _filterSubject.OnNext(FilterPrototypes);
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
    /// Handles filtering prototypes based on their kind and the current filtered text.
    /// </summary>
    /// <param name="prototype">The view model of the prototype to be filtered.</param>
    /// <returns>True if the prototype should be visible, otherwise false.</returns>
    private bool FilterPrototypes(PrototypeViewModel prototype)
    {
        // First check whether this prototype is even in the correct category
        if (!ListFilterKind.Equals(_defaultAll) && ListFilterKind.Kind != prototype.Kind)
        {
            return false;
        }

        // Then perform a string check against the ID of the prototype
        // TODO: Possibly improve fuzzy finding and string validation
        if (!string.IsNullOrWhiteSpace(ListFilterText) && !prototype.Id.Contains(ListFilterText))
        {
            return false;
        }

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
        Kinds.Add(_defaultAll);
        foreach (var kind in _prototypeProvider.GetKinds())
        {
            Kinds.Add(kind);
        }
    }
}
