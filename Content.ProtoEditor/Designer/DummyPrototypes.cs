using System.Collections.Generic;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.Designer;

/// <summary>
/// Dummy Enum for showing enum fields
/// </summary>
public enum DummyEnum
{
    DummyVal0,
    DummyVal1,
    DummyVal2,
    DummyVal3,
}

/// <summary>
/// Dummy class for showing a fallback field in the design view/preview for Avalonia
/// </summary>
public sealed class ForcedFallback
{
    public string Value = "Fallback";
}

public abstract class DummyBase
{
    // ReSharper disable once InconsistentNaming
    public string camelCasedField = "camelCase to Camel Case";
    public int IntegerField = 0;
    public uint UnsignedIntegerField = 0;
    public bool BooleanField = true;
    public ForcedFallback FallbackField = new();
    public string StringField = "StringField";
    public string? NullableStringField = null;
    public float FloatField = 0f;
    public List<int> ArrayField = [0, 1, 2, 3];
    public DummyEnum EnumField = DummyEnum.DummyVal0;
}

/// <summary>
/// Dummy prototype to show in the design view/preview for Avalonia
/// </summary>
/// <param name="id">The ID for this instance of the prototype.</param>
public sealed class PlanetPrototypes(string id) : DummyBase, IPrototype
{
    public string ID { get; } = id;
}

/// <summary>
/// Another kind of prototype for design view/preview for Avalonia
/// </summary>
/// <param name="id">The ID for this instance of the prototype.</param>
public sealed class DwarfsPrototype(string id) : DummyBase, IPrototype
{
    public string ID { get; } = id;
}

/// <summary>
/// A prototype kind that should not have any instances attached to it.
/// </summary>
/// <param name="id">The ID for this instance of the prototype.</param>
public sealed class EmptyPrototype(string id) : DummyBase, IPrototype
{
    public string ID { get; } = id;
}
