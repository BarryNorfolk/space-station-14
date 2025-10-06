using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.ProtoEditor.ViewModels;
using DynamicData;
using DynamicData.Kernel;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.Services;

/// <summary>
/// Provides access to Prototypes and their kinds, loaded from the prototype manager.
/// </summary>
public sealed class PrototypeProvider : IPrototypeProvider
{
    /// <summary>
    /// List of field names from mapping nodes to ignore when enumerating inherited fields
    /// from parent prototypes.
    /// </summary>
    private readonly List<string> _ignoredInheritedFields =
    [
        "id",
        "name",
        "abstract",
    ];

    /// <summary>
    /// List of all the "Kinds" (or types) of Prototypes, wrapped in a PrototypeKind class
    /// for easier handling within UI code.
    /// </summary>
    private readonly List<PrototypeKindViewModel> _kinds = [];

    /// <summary>
    /// Stored resolution of the Prototype manager from the Server assembly.
    /// </summary>
    private readonly EditorPrototypeManager _prototypeManager;

    /// <summary>
    /// The worker thread with all IoC dependencies initialized on it, used to background
    /// certain work tasks and not block UI.
    /// </summary>
    private readonly BackgroundWorkerProvider _worker;

    public PrototypeProvider(DependencyProvider assembly, BackgroundWorkerProvider worker)
    {
        _worker = worker;

        _prototypeManager = (EditorPrototypeManager)assembly.Resolve<IPrototypeManager>();
        _prototypeManager.Initialize();
        _prototypeManager.RegisterIgnore("parallax");

        Initialization = _worker.RunAsync(LoadPrototypes);
    }

    public Task Initialization { get; private set; }

    public Optional<PrototypeViewModel> GetPrototypeModel(string id)
    {
        foreach (var prototype in _kinds.Select(kind => kind.Prototypes.Lookup(id.GetHashCode()))
                     .Where(prototype => prototype.HasValue))
        {
            return prototype;
        }

        return Optional<PrototypeViewModel>.None;
    }

    public List<PrototypeKindViewModel> GetKinds()
    {
        return _kinds;
    }

    public async Task ForceReload()
    {
        var modified = new Dictionary<Type, HashSet<string>>();
        _prototypeManager.ReloadPrototypes(modified);
        _kinds.Clear();

        await _worker.RunAsync(LoadPrototypes);
    }

    public Dictionary<string, List<InheritedFieldData>> GetBaseFields(PrototypeViewModel prototype)
    {
        return _prototypeManager.EnumerateBaseFields(prototype.Kind, prototype.Id, _ignoredInheritedFields);
    }

    public string GetParents(PrototypeViewModel prototype)
    {
        if (!prototype.Kind.IsAssignableTo(typeof(IInheritingPrototype)))
            return "Not inherited";

        var f = new StringBuilder();
        foreach (var parentId in _prototypeManager.EnumerateAllParents(prototype.Kind, prototype.Id))
        {
            f.Append(parentId);
            f.Append(',');
        }

        return f.ToString();
    }

    /// <summary>
    /// Loads and processes Prototypes and their Kinds from the resolved prototype manager.
    /// </summary>
    private Task LoadPrototypes()
    {
        _prototypeManager.LoadDefaultPrototypes();

        foreach (var kind in _prototypeManager.EnumeratePrototypeKinds())
        {
            var kindVm = new PrototypeKindViewModel(kind);
            foreach (var prototype in _prototypeManager.EnumeratePrototypes(kind))
            {
                kindVm.Prototypes.AddOrUpdate(new PrototypeViewModel(prototype, kind));
            }

            _kinds.Add(kindVm);
        }

        _kinds.Sort((lhs, rhs) => lhs.ShortName.CompareTo(rhs.ShortName));

        return Task.CompletedTask;
    }
}
