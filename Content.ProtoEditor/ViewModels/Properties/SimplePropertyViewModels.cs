using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using YamlDotNet.Core.Tokens;

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
    private string? _name = null;

    /// <summary>
    /// Whether this property is part of an array.
    /// </summary>
    [ObservableProperty]
    private bool _isArrayElement = false;

    public PropertyViewModel(string? name)
    {
        Name = name;
    }
}

/// <summary>
/// Simple string view model.
/// </summary>
/// <param name="name">Name of this property, nullable if part of an array.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor(typeof(string))]
public sealed partial class StringPropertyViewModel(string? name, string? value) : PropertyViewModel(name)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private string? _value = value;
}

/// <summary>
/// Simple boolean view model.
/// </summary>
/// <param name="name">Name of this property, nullable if part of an array.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor(typeof(bool))]
public sealed partial class BoolPropertyViewModel(string? name, bool? value) : PropertyViewModel(name)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private bool? _value = value;
}

/// <summary>
/// Simple signed integer view model.
/// </summary>
/// <param name="name">Name of this property, nullable if part of an array.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor([typeof(char), typeof(short), typeof(int), typeof(long)])]
public sealed partial class IntPropertyViewModel(string? name, long value) : PropertyViewModel(name)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private long _value = value;
}

/// <summary>
/// Simple unsigned integer view model.
/// </summary>
/// <param name="name">Name of this property, nullable if part of an array.</param>
/// <param name="value">Initial value of this property.</param>
[ViewModelFor([typeof(ushort), typeof(uint), typeof(ulong)])]
public sealed partial class UIntPropertyViewModel(string? name, ulong value) : PropertyViewModel(name)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private ulong _value = value;
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

    public EnumPropertyViewModel(string? name, Enum? value) : base(name)
    {
        Value = value?.ToString() ?? "None";
        Options = value != null ? new(Enum.GetNames(value.GetType())) : [];
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
public sealed partial class FallbackPropertyViewModel(string? name, string value) : PropertyViewModel(name)
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private string _value = value;
}
