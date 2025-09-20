using System;
using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.ViewModels.Properties;

/// <summary>
/// Simple string view model.
/// </summary>
/// <param name="info">Info about an attribute of a class.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor(typeof(string))]
[UsedImplicitly]
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
/// <param name="info">Info about an attribute of a class.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor(typeof(bool))]
[UsedImplicitly]
public sealed partial class BoolPropertyViewModel(MemberInfo info, bool? value) : PropertyViewModel(info)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private bool? _value = value;

    public override void SaveToInstance(IPrototype instance) { Save(instance, Value); }
}

[ViewModelFor([typeof(char), typeof(short), typeof(int), typeof(long)])]
[UsedImplicitly]
public sealed partial class IntPropertyViewModel : PropertyViewModel
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private long _value;

    /// <summary>
    /// Simple signed integer view model.
    /// </summary>
    /// <param name="info">Info about an attribute of a class.</param>
    /// <param name="value">Initial value of this property.</param>
    public IntPropertyViewModel(MemberInfo info, object value) : base(info)
    {
        Value = Convert.ToInt64(value);

        switch (TypeCode)
        {
            case TypeCode.SByte:
                Tooltip = string.Format("An 8-bit integer");
                break;
            case TypeCode.Int16:
                Tooltip = string.Format("A 16-bit integer");
                break;
            case TypeCode.Int32:
                Tooltip = string.Format("A 32-bit integer");
                break;
            case TypeCode.Int64:
                Tooltip = string.Format("A 64-bit integer");
                break;
        }
    }

    public override void SaveToInstance(IPrototype instance)
    {
        switch (TypeCode)
        {
            case TypeCode.SByte:
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

[ViewModelFor([typeof(ushort), typeof(uint), typeof(ulong)])]
[UsedImplicitly]
public sealed partial class UIntPropertyViewModel : PropertyViewModel
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private ulong _value;

    /// <summary>
    /// Simple unsigned integer view model.
    /// </summary>
    /// <param name="info">Info about an attribute of a class.</param>
    /// <param name="value">Initial value of this property.</param>
    public UIntPropertyViewModel(MemberInfo info, object value) : base(info)
    {
        Value = Convert.ToUInt64(value);

        switch (TypeCode)
        {
            case TypeCode.Byte:
                Tooltip = string.Format("An 8-bit unsigned integer");
                break;
            case TypeCode.UInt16:
                Tooltip = string.Format("A 16-bit unsigned integer");
                break;
            case TypeCode.UInt32:
                Tooltip = string.Format("A 32-bit unsigned integer");
                break;
            case TypeCode.UInt64:
                Tooltip = string.Format("A 64-bit unsigned integer");
                break;
        }
    }

    public override void SaveToInstance(IPrototype instance)
    {
        switch (TypeCode)
        {
            case TypeCode.Byte: // 8 bit Unsigned
                Save(instance, Convert.ToSByte(Value));
                break;
            case TypeCode.UInt16:
                Save(instance, Convert.ToUInt16(Value));
                break;
            case TypeCode.UInt32:
                Save(instance, Convert.ToUInt32(Value));
                break;
            case TypeCode.UInt64:
                Save(instance, Convert.ToUInt64(Value));
                break;
            default:
                throw new NotImplementedException();
        }
    }
}

[ViewModelFor(typeof(Enum))]
[UsedImplicitly]
public sealed partial class EnumPropertyViewModel : PropertyViewModel
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private string? _value;

    /// <summary>
    /// Simple enum view model.
    /// </summary>
    /// <param name="info">Info about an attribute of a class.</param>
    /// <param name="value">Initial value of this property.</param>
    public EnumPropertyViewModel(MemberInfo info, Enum? value) : base(info)
    {
        Value = value?.ToString() ?? "None";
        Options = value != null ? new ObservableCollection<string>(Enum.GetNames(value.GetType())) : [];
    }

    /// <summary>
    /// Possible options for this Enum.
    /// </summary>
    public ObservableCollection<string> Options { get; }

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
/// implicitly.
/// </summary>
/// <param name="info">Info about an attribute of a class.</param>
/// <param name="value">Initial value of this property.</param>
[UsedImplicitly]
public sealed partial class FallbackPropertyViewModel(MemberInfo info, string value) : PropertyViewModel(info)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private string _value = value;

    public override void SaveToInstance(IPrototype instance) { Save(instance, Value); }
}
