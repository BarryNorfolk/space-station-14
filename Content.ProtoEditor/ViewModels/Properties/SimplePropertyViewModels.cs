using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Content.ProtoEditor.Services;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.ProtoEditor.ViewModels.Properties;

/// <summary>
/// Base ViewModel for all properties/fields that are part of a prototype
/// </summary>
public abstract partial class PropertyViewModel : ViewModelBase
{
    /// <summary>
    /// Name of this property, optional for the case where this property view model is
    /// used in the context of an array.
    /// </summary>
    [ObservableProperty]
    private string _name;

    /// <summary>
    /// Whether this property is part of an array.
    /// </summary>
    [ObservableProperty]
    private bool _isArrayElement = false;

    /// <summary>
    /// Whether this property cannot be changed by the editor.
    /// </summary>
    [ObservableProperty]
    private bool _isFrozen = false;

    [ObservableProperty]
    private string? _inheritedFields = null;

    /// <summary>
    /// Stored reference to the member (Field or Property) information on the Prototype.
    /// </summary>
    protected readonly MemberInfo MemberInfo;

    protected readonly TypeCode TypeCode;

    public PropertyViewModel(MemberInfo info)
    {
        Name = GetName(info);
        TypeCode = GetTypeCode(info);
        MemberInfo = info;
    }

    public abstract void SaveToInstance(IPrototype instance);

    protected void Save(IPrototype instance, object? value)
    {
        if (MemberInfo.MemberType == MemberTypes.Field)
        {
            ((FieldInfo)MemberInfo).SetValue(instance, value);
        }
        else if (MemberInfo.MemberType == MemberTypes.Property)
        {
            ((PropertyInfo)MemberInfo).SetValue(instance, value);
        }
    }

    protected static Type GetType(MemberInfo info)
    {
        return info.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)info).FieldType,
            MemberTypes.Property => ((PropertyInfo)info).PropertyType,
            _ => throw new NotImplementedException(), // TODO: Better exception
        };
    }

    protected static TypeCode GetTypeCode(MemberInfo info)
    {
        return info.MemberType switch
        {
            MemberTypes.Field => Type.GetTypeCode(((FieldInfo)info).FieldType),
            MemberTypes.Property => Type.GetTypeCode(((PropertyInfo)info).PropertyType),
            _ => throw new NotImplementedException(), // TODO: Better exception
        };
    }

    protected static string GetName(MemberInfo info)
    {
        if (info.TryGetCustomAttribute<DataFieldAttribute>(out var field) &&
            field != null &&
            field.Tag != null)
        {
            // Tags override names of the properties
            return field.Tag;
        }

        return info.Name;
    }

    public virtual void SetInheritedFields(List<InheritedFieldData> inheritedFields)
    {
        if (inheritedFields.Count == 0)
            return;

        var s = new StringBuilder();
        foreach (var d in inheritedFields)
        {
            s.Append($"Inherits [{d.Value}] from [{d.Id}]\n");
        }

        InheritedFields = s.ToString();
    }
}

/// <summary>
/// Simple string view model.
/// </summary>
/// <param name="name">Name of this property, nullable if part of an array.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor(typeof(string))]
public sealed partial class StringPropertyViewModel(MemberInfo info, string? value) : PropertyViewModel(info)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private string? _value = value;

    public override void SaveToInstance(IPrototype instance) { Save(instance, Value); }
}

/// <summary>
/// Simple boolean view model.
/// </summary>
/// <param name="name">Name of this property, nullable if part of an array.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor(typeof(bool))]
public sealed partial class BoolPropertyViewModel(MemberInfo info, bool? value) : PropertyViewModel(info)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private bool? _value = value;

    public override void SaveToInstance(IPrototype instance) { Save(instance, Value); }
}

/// <summary>
/// Simple signed integer view model.
/// </summary>
/// <param name="name">Name of this property, nullable if part of an array.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor([typeof(char), typeof(short), typeof(int), typeof(long)])]
public sealed partial class IntPropertyViewModel : PropertyViewModel
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private long _value;

    public IntPropertyViewModel(MemberInfo info, object value) : base(info)
    {
        Value = Convert.ToInt64(value);
    }

    public override void SaveToInstance(IPrototype instance)
    {
        switch (TypeCode)
        {
            case TypeCode.Char:
                Save(instance, Convert.ToChar(Value));
                break;
            case TypeCode.Int16:
                Save(instance, Convert.ToInt16(Value));
                break;
            case TypeCode.Int32:
                Save(instance, Convert.ToInt32(Value));
                break;
            case TypeCode.Int64:
                Save(instance, Convert.ToInt64(Value));
                break;
            default:
                throw new NotImplementedException();
        }
    }
}

/// <summary>
/// Simple unsigned integer view model.
/// </summary>
/// <param name="name">Name of this property, nullable if part of an array.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor([typeof(ushort), typeof(uint), typeof(ulong)])]
public sealed partial class UIntPropertyViewModel(MemberInfo info, ulong value) : PropertyViewModel(info)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private ulong _value = value;

    public override void SaveToInstance(IPrototype instance)
    {
        switch (TypeCode)
        {
            case TypeCode.SByte:    // 8 bit Unsigned
                Save(instance, Convert.ToSByte(Value));
                break;
            case TypeCode.UInt16:
                Save(instance, Convert.ToUInt16(Value));
                break;
            case TypeCode.UInt32:
                Save(instance, Convert.ToInt32(Value));
                break;
            case TypeCode.UInt64:
                Save(instance, Convert.ToInt64(Value));
                break;
            default:
                throw new NotImplementedException();
        }
    }
}

/// <summary>
/// Simple enum view model.
/// </summary>
/// <param name="name">Name of this property, nullable if part of an array.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor(typeof(Enum))]
public sealed partial class EnumPropertyViewModel : PropertyViewModel
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private string? _value;

    /// <summary>
    /// Possible options for this Enum.
    /// </summary>
    public ObservableCollection<string> Options { get; }

    public EnumPropertyViewModel(MemberInfo info, Enum? value) : base(info)
    {
        Value = value?.ToString() ?? "None";
        Options = value != null ? new(Enum.GetNames(value.GetType())) : [];
    }

    public override void SaveToInstance(IPrototype instance)
    {
        if (Enum.TryParse(GetType(), Value, out var obj))
            Save(instance, obj);
        else
        {
            // TODO: What's a "default" value for an Enum look like here?
            //       For the case where the string value is None.
            Save(instance, null);
        }
    }
}

/// <summary>
/// Fallback property view model, in the case where no other view models can support/render
/// the created type.
/// N.b. The lack of ViewModelFor is intentional, as we don't want this view model to be used
///      implicitly.
/// </summary>
/// <param name="name">Name of this property, nullable if part of an array.</param>
/// <param name="value">Initial value of this property.</param>
public sealed partial class FallbackPropertyViewModel(MemberInfo info, string value) : PropertyViewModel(info)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private string _value = value;

    public override void SaveToInstance(IPrototype instance) { Save(instance, Value); }
}
