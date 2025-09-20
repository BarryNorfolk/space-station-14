using System;

namespace Content.ProtoEditor.ViewModels.Properties;

/// <summary>
/// Attribute denoting what types a particular PropertyViewModel derived class can support.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ViewModelForAttribute : Attribute
{
    public ViewModelForAttribute(Type[] types)
    {
        TargetTypes = types;
    }

    public ViewModelForAttribute(Type type)
    {
        TargetTypes = [type];
    }

    /// <summary>
    /// The supported types for this attribute.
    /// </summary>
    public Type[] TargetTypes { get; }
}
