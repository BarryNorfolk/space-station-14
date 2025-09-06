using System;
using System.Collections.Generic;
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
    /// The worker thread with all IoC dependencies initialized on it, used to background
    /// certain work tasks and not block UI.
    /// </summary>
    private readonly BackgroundWorkerProvider _worker;

    /// <summary>
    /// Stored resolution of the Prototype manager from the Server assembly.
    /// </summary>
    private IPrototypeManager _prototypeManager;

    /// <summary>
    /// Stored Prototypes wrapped in a ViewModel, ready for use in Views/UI.
    /// </summary>
    private readonly SourceCache<PrototypeViewModel, int> _prototypes = new(x => x.Id.GetHashCode());

    /// <summary>
    /// List of all the "Kinds" (or types) of Prototypes, wrapped in a PrototypeKind class
    /// for easier handling within UI code.
    /// </summary>
    private readonly List<PrototypeKind> _kinds = [];

    public PrototypeProvider(DependencyProvider assembly, BackgroundWorkerProvider worker)
    {
        _assembly = assembly;
        _worker = worker;

        _prototypeManager = _assembly.Resolve<IPrototypeManager>();
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
    ///
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
}
