using System.Collections.Generic;
using System.Threading.Tasks;
using Content.ProtoEditor.Models;
using Content.ProtoEditor.ViewModels;
using DynamicData;
using DynamicData.Kernel;

namespace Content.ProtoEditor.Services;

public interface IPrototypeProvider
{
    /// <summary>
    /// Exposes the initialization task so that other async tasks may await on it.
    /// </summary>
    Task Initialization { get; }

    /// <summary>
    /// Gets the current cache of Prototypes.
    /// </summary>
    /// <returns>SourceCache for all loaded prototypes.</returns>
    SourceCache<PrototypeViewModel, int> GetPrototypeModels();

    /// <summary>
    /// Tries to resolve a prototype ID to an existing, loaded, view model.
    /// </summary>
    /// <param name="id">The prototype ID to search for.</param>
    /// <returns>The found view model, otherwise null.</returns>
    Optional<PrototypeViewModel> GetPrototypeModel(string id);

    /// <summary>
    /// </summary>
    /// <returns>List of all loaded prototype Kinds</returns>
    List<PrototypeKind> GetKinds();

    /// <summary>
    /// Forces a reload of all prototypes and their kinds.
    /// </summary>
    Task ForceReload();

    Dictionary<string, List<InheritedFieldData>> GetBaseFields(PrototypeViewModel prototype);

    /*
        Ok so, the abstracts DON'T Get any information in the
        EnumerateAllParents because they don't exist in the prototype
        manager. That's weird I guess.
        Investigate how the inheritance works with merging properties
        from abstract prototypes that don't actually have anything.
    */
    string GetParents(PrototypeViewModel prototype);
}
