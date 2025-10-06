using System;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Content.ProtoEditor.ViewModels;

public sealed partial class PrototypeKindViewModel : ViewModelBase
{
    /// <summary>
    /// Simple wrapper around a prototype Kind to enable nice visibility in UI,
    /// and filtering of the list of prototypes.
    /// </summary>
    /// <param name="kind">The actual underlying type for the IPrototype kind.</param>
    public PrototypeKindViewModel(Type kind)
    {
        Kind = kind;
        Name = Kind.FullName ?? Kind.ToString();
        ShortName = Kind.ShortDisplayName();

        Prototypes.Connect()
            .AutoRefresh(p => p.Id)
            .Filter(FilterSubject)
            .Sort(SortExpressionComparer<PrototypeViewModel>.Ascending(p => p.Id))
            .Bind(out _filteredPrototypes)
            .Subscribe();
    }

    public bool HasItems => FilteredPrototypes.Count != 0;

    /// <summary>
    /// The actual type for the IPrototype, as determined by the IPrototypeManager.
    /// </summary>
    public readonly Type Kind;

    /// <summary>
    /// Full name of the prototype kind.
    /// E.g. "Content.Shared.Store.ListingPrototype".
    /// </summary>
    public string Name;

    /// <summary>
    /// Simple short name for viewing in UI.
    /// E.g. "Content.Shared.Store.ListingPrototype" becomes "ListingPrototype".
    /// </summary>
    [ObservableProperty]
    private string _shortName;

    /// <summary>
    /// Cache of all Prototypes attached to this Kind
    /// </summary>
    public readonly SourceCache<PrototypeViewModel, int> Prototypes = new(x => x.Id.GetHashCode());

    /// <summary>
    /// Collection that has been filtered/sorted and connected to the list of available prototypes
    /// in the PrototypeManager.
    /// </summary>
    private readonly ReadOnlyObservableCollection<PrototypeViewModel> _filteredPrototypes;

    public ReadOnlyObservableCollection<PrototypeViewModel> FilteredPrototypes => _filteredPrototypes;

    /// <summary>
    /// Used to enable dynamic filtering of the source prototype list.
    /// </summary>
    public readonly BehaviorSubject<Func<PrototypeViewModel, bool>> FilterSubject =
        new(p => true);
}
