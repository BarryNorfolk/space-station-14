using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Content.ProtoEditor.Models;
using Content.ProtoEditor.ViewModels;
using DynamicData;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.Services;

/// <summary>
/// Provides access to Prototypes and their kinds, loaded from the prototype manager.
/// </summary>
public sealed class PrototypeProvider
{
    /// <summary>
    /// Internal Assembly provider that gives access to the actual PrototypeManager
    /// </summary>
    private readonly DependencyProvider _assembly;

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
    private readonly List<PrototypeKind> _kinds = [];

    /// <summary>
    /// Stored resolution of the Prototype manager from the Server assembly.
    /// </summary>
    private readonly EditorPrototypeManager _prototypeManager;

    /// <summary>
    /// Stored Prototypes wrapped in a ViewModel, ready for use in Views/UI.
    /// </summary>
    private readonly SourceCache<PrototypeViewModel, int> _prototypes = new(x => x.Id.GetHashCode());

    /// <summary>
    /// The worker thread with all IoC dependencies initialized on it, used to background
    /// certain work tasks and not block UI.
    /// </summary>
    private readonly BackgroundWorkerProvider _worker;

    public PrototypeProvider(DependencyProvider assembly, BackgroundWorkerProvider worker)
    {
        _assembly = assembly;
        _worker = worker;

        _prototypeManager = (EditorPrototypeManager)assembly.Resolve<IPrototypeManager>();
        _prototypeManager.Initialize();
        _prototypeManager.RegisterIgnore("parallax");

        Initialization = _worker.RunAsync(LoadPrototypes);
    }

    /// <summary>
    /// Exposes the initialization task so that other async tasks may await on it.
    /// </summary>
    public Task Initialization { get; private set; }

    /// <summary>
    /// Gets the current cache of Prototypes.
    /// </summary>
    /// <returns>SourceCache for all loaded prototypes.</returns>
    public SourceCache<PrototypeViewModel, int> GetPrototypeModels()
    {
        return _prototypes;
    }

    /// <summary>
    /// </summary>
    /// <returns>List of all loaded prototype Kinds</returns>
    public List<PrototypeKind> GetKinds()
    {
        return _kinds;
    }

    /// <summary>
    /// Loads and processes Prototypes and their Kinds from the resolved prototype manager.
    /// </summary>
    private Task LoadPrototypes()
    {
        _prototypeManager.LoadDefaultPrototypes();

        foreach (var kind in _prototypeManager.EnumeratePrototypeKinds())
        {
            _kinds.Add(new PrototypeKind(kind));
            foreach (var prototype in _prototypeManager.EnumeratePrototypes(kind))
            {
                _prototypes.AddOrUpdate(new PrototypeViewModel(prototype, kind));
            }
        }

        _kinds.Sort((lhs, rhs) => lhs.ShortName.CompareTo(rhs.ShortName));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Forces a reload of all prototypes and their kinds.
    /// </summary>
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

    /*
        Ok so, the abstracts DON'T Get any information in the
        EnumerateAllParents because they don't exist in the prototype
        mananger. That's weird I guess.
        Investigate how the inheritance works with merging properties
        from abstract prototypes that don't actually have anything.
    */
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
}
