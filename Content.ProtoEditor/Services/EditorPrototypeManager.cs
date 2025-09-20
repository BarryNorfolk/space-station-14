using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Utility;

namespace Content.ProtoEditor.Services;

/// <summary>
/// Holds data related to a specific field, which parent prototype it came from
/// and the value.
/// </summary>
/// <param name="id">Prototype ID of the parent prototype.</param>
/// <param name="value">Value of the field that was inherited.</param>
public sealed class InheritedFieldData(string id, DataNode value)
{
    /// <summary>
    /// The prototype ID where the value of this came from.
    /// </summary>
    public readonly string Id = id;

    /// <summary>
    /// The value of the field that was inherited.
    /// </summary>
    public readonly DataNode Value = value;
}

/// <summary>
/// Specialized prototype manager for the Editor.
/// </summary>
[UsedImplicitly]
public sealed class EditorPrototypeManager : PrototypeManager
{
    private readonly Dictionary<Type, List<AbstractPrototype>> _abstractPrototypes = [];

    public EditorPrototypeManager()
    {
        RegisterIgnore("shader");
        RegisterIgnore("uiTheme");
        RegisterIgnore("font");
    }

    private void ProcessAbstractPrototypes()
    {
        foreach (var (kind, data) in _kinds)
        {
            foreach (var (id, mapping) in data.RawResults)
            {
                if (!mapping.TryGetValue(AbstractDataFieldAttribute.Name, out var isAbstract))
                    continue;

                if (!_serializationManager.Read<bool>(isAbstract))
                    continue;

                _abstractPrototypes.GetOrNew(kind).Add(new AbstractPrototype(id, mapping));
            }
        }
    }

    public override void LoadDefaultPrototypes(Dictionary<Type, HashSet<string>>? changed = null)
    {
        LoadDirectory(new ResPath("/EnginePrototypes/"), changed: changed);
        LoadDirectory(new ResPath("/Prototypes/"), changed: changed); // TODO: Make possibly configurable?
        ProcessAbstractPrototypes();
        ResolveResults();
    }

    public Dictionary<string, List<InheritedFieldData>> EnumerateBaseFields(Type kind,
        string id,
        List<string> ignoredFields)
    {
        Dictionary<string, List<InheritedFieldData>> data = [];

        if (_kinds[kind].Inheritance == null)
            return data;

        // We want even the abstract ones
        foreach (var (parent, _) in EnumerateAllParents(kind, id))
        {
            var parentData = _kinds[kind].RawResults[parent];
            foreach (var (fieldName, node) in parentData)
            {
                if (ignoredFields.Contains(fieldName))
                    continue;

                data.GetOrNew(fieldName).Add(new InheritedFieldData(parent, node));
            }
        }

        return data;
    }

    public IEnumerable<(string id, IPrototype?)> EnumerateAllParents(Type kind, string id, bool includeSelf = false)
    {
        if (!_hasEverBeenReloaded)
            throw new InvalidOperationException("No prototypes have been loaded yet.");

        if (!kind.IsAssignableTo(typeof(IInheritingPrototype)))
            throw new InvalidOperationException("The provided prototype type is not an inheriting prototype");

        if (!_kinds.TryGetValue(kind, out var kindData))
            throw new UnknownPrototypeException(id, kind);

        if (!kindData.Results.ContainsKey(id))
            yield break;

        IPrototype? uncast;

        if (includeSelf)
        {
            kindData.Instances.TryGetValue(id, out uncast);
            yield return (id, uncast);
        }

        if (!kindData.Inheritance!.TryGetParents(id, out var parents))
            yield break;

        var queue = new Queue<string>(parents);
        while (queue.TryDequeue(out var prototypeId))
        {
            if (!kindData.Results.ContainsKey(prototypeId))
            {
                Sawmill.Error(
                    $"Encountered invalid prototype while enumerating parents. Kind: {kind.Name}. Child: {id}. Invalid: {prototypeId}");
                continue;
            }

            kindData.Instances.TryGetValue(prototypeId, out uncast);
            yield return (prototypeId, uncast);

            if (!kindData.Inheritance.TryGetParents(prototypeId, out parents))
                continue;

            foreach (var parentId in parents)
            {
                queue.Enqueue(parentId);
            }
        }
    }

    private sealed class AbstractPrototype(string id, MappingDataNode data)
    {
        public MappingDataNode Data = data;
        public string Id = id;
    }
}
