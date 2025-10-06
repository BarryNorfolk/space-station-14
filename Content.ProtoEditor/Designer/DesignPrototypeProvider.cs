using Content.ProtoEditor.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    private readonly List<PrototypeKindViewModel> _kinds = [];

    public DesignPrototypeProvider()
    {
        var planets = new PrototypeKindViewModel(typeof(PlanetPrototypes));
        // TODO: Figure out how to suppress the Robust check for prototype instantiation here
        planets.Prototypes.AddOrUpdate(
            new PrototypeViewModel(new PlanetPrototypes("Mercury"), typeof(PlanetPrototypes)));
        planets.Prototypes.AddOrUpdate(new PrototypeViewModel(new PlanetPrototypes("Venus"), typeof(PlanetPrototypes)));
        planets.Prototypes.AddOrUpdate(new PrototypeViewModel(new PlanetPrototypes("Earth"), typeof(PlanetPrototypes)));
        planets.Prototypes.AddOrUpdate(new PrototypeViewModel(new PlanetPrototypes("Mars"), typeof(PlanetPrototypes)));
        planets.Prototypes.AddOrUpdate(
            new PrototypeViewModel(new PlanetPrototypes("Jupiter"), typeof(PlanetPrototypes)));
        planets.Prototypes.AddOrUpdate(new PrototypeViewModel(new PlanetPrototypes("Saturn"),
            typeof(PlanetPrototypes)));
        planets.Prototypes.AddOrUpdate(new PrototypeViewModel(new PlanetPrototypes("Uranus"),
            typeof(PlanetPrototypes)));
        planets.Prototypes.AddOrUpdate(
            new PrototypeViewModel(new PlanetPrototypes("Neptune"), typeof(PlanetPrototypes)));
        _kinds.Add(planets);

        // Empty in the middle to test showing empty kinds during filtering
        _kinds.Add(new PrototypeKindViewModel(typeof(EmptyPrototype)));

        var dwarfs = new PrototypeKindViewModel(typeof(DwarfsPrototype));
        dwarfs.Prototypes.AddOrUpdate(new PrototypeViewModel(new DwarfsPrototype("Pluto"), typeof(DwarfsPrototype)));
        dwarfs.Prototypes.AddOrUpdate(new PrototypeViewModel(new DwarfsPrototype("Ceres"), typeof(DwarfsPrototype)));
        dwarfs.Prototypes.AddOrUpdate(new PrototypeViewModel(new DwarfsPrototype("Eris"), typeof(DwarfsPrototype)));
        dwarfs.Prototypes.AddOrUpdate(new PrototypeViewModel(new DwarfsPrototype("MakeMake"), typeof(DwarfsPrototype)));
        _kinds.Add(dwarfs);
    }

    /// <summary>
    /// Gets the first dummy prototype available in the source cache.
    /// </summary>
    /// <returns>A dummy prototype for use in design views.</returns>
    public PrototypeViewModel GetDummyPrototype()
    {
        return _kinds.First().Prototypes.KeyValues.First().Value;
    }

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
