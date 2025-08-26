using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Content.ProtoEditor.Models;
using Content.ProtoEditor.ViewModels;
using DynamicData;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.Services;

/// <summary>
/// Provides access to Prototypes and their kinds, loaded from the resolved assembly.
/// </summary>
public sealed class PrototypeProvider
{
    /// <summary>
    /// Internal Assembly provider that gives access to the actual PrototypeManager from the real
    /// Server/Client.
    /// </summary>
    private readonly AssemblyProvider _assembly;

    /// <summary>
    /// Stored resolution of the Prototype manager from the Server assembly.
    /// </summary>
    private IPrototypeManager? _prototypeManager;

    /// <summary>
    /// Stored Prototypes wrapped in a ViewModel, ready for use in Views/UI.
    /// </summary>
    private SourceCache<PrototypeViewModel, int> _prototypes = new(x => x.Id.GetHashCode());

    /// <summary>
    /// List of all the "Kinds" (or types) of Prototypes, wrapped in a PrototypeKind class
    /// for easier handling within UI code.
    /// </summary>
    private readonly List<PrototypeKind> _kinds = [];

    public PrototypeProvider(AssemblyProvider assembly)
    {
        _assembly = assembly;
        Initialization = InitializeAsync();
    }

    /// <summary>
    /// Exposes the initialization task so that other async tasks may await on it.
    /// </summary>
    public Task Initialization { get; private set; }

    /// <summary>
    /// Initializes this class.
    /// Awaits the assembly to be ready, resolves the PrototypeManager from there, and then
    /// loads the prototypes and their kinds.
    /// </summary>
    /// <returns>The initialization task</returns>
    private async Task InitializeAsync()
    {
        await _assembly.Initialization;

        var server = _assembly.Server();
        if (server == null)
            return;

        _prototypeManager = server.Resolve<IPrototypeManager>();
        LoadPrototypes();
    }

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
    private void LoadPrototypes()
    {
        if (_prototypeManager == null)
            return;

        foreach (var kind in _prototypeManager.EnumeratePrototypeKinds())
        {
            _kinds.Add(new PrototypeKind(kind));
            foreach (var prototype in _prototypeManager.EnumeratePrototypes(kind))
            {
                _prototypes.AddOrUpdate(new PrototypeViewModel(prototype, kind));
            }
        }

        _kinds.Sort((lhs, rhs) => lhs.ShortName.CompareTo(rhs.ShortName));
    }

    /// <summary>
    /// Forces a reload of all prototypes and their kinds.
    /// </summary>
    public void ForceReload()
    {
        if (_prototypeManager == null)
            return;

        var modified = new Dictionary<Type, HashSet<string>>();
        _prototypeManager.ReloadPrototypes(modified);
        _kinds.Clear();
        LoadPrototypes();
    }
}
