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
/// Dummy prototype to show in the design view/preview for Avalonia
/// </summary>
public sealed class DummyPrototype : IPrototype
{
    public string ID { get; } = "DummyPrototype";

    public int IntegerField = 0;
    public bool BooleanField = true;
    public string StringField = "StringField";
    public float FloatField = 0f;
    public DummyEnum EnumField = DummyEnum.DummyVal0;
    public List<int> ArrayField = [0, 1, 2, 3];
}
