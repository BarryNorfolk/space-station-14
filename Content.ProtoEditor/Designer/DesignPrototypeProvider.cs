using Content.ProtoEditor.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Content.ProtoEditor.Models;
using Content.ProtoEditor.ViewModels;
using DynamicData;
using DynamicData.Kernel;

namespace Content.ProtoEditor.Designer;

/// <summary>
/// Specialised class for injecting prototypes in the design view/preview
/// for avalonia.
/// </summary>
public sealed class DesignPrototypeProvider : IPrototypeProvider
{
    public Task Initialization { get; } = Task.CompletedTask;

    private readonly List<PrototypeKind> _kinds = [];

    private readonly SourceCache<PrototypeViewModel, int> _prototypes = new(x => x.Id.GetHashCode());

    public DesignPrototypeProvider()
    {
        // TODO: Figure out how to suppress the Robust check for prototype instantiation here
        _prototypes.AddOrUpdate(new PrototypeViewModel(new DummyPrototype(), typeof(DummyPrototype)));
    }

    public SourceCache<PrototypeViewModel, int> GetPrototypeModels()
    {
        return _prototypes;
    }

    public Optional<PrototypeViewModel> GetPrototypeModel(string id)
    {
        return _prototypes.Lookup(id.GetHashCode());
    }

    public List<PrototypeKind> GetKinds()
    {
        return _kinds;
    }

    public Task ForceReload()
    {
        return Task.CompletedTask;
    }

    public Dictionary<string, List<InheritedFieldData>> GetBaseFields(PrototypeViewModel prototype)
    {
        return new Dictionary<string, List<InheritedFieldData>>();
    }

    public string GetParents(PrototypeViewModel prototype)
    {
        return "";
    }
}
