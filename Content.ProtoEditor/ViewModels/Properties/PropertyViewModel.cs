using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using Content.ProtoEditor.Services;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.ProtoEditor.ViewModels.Properties;

/// <summary>
/// Base ViewModel for all properties/fields that are part of a prototype
/// </summary>
public abstract partial class PropertyViewModel : ViewModelBase, INotifyDataErrorInfo
{
    private static readonly Regex WordRegex =
        new(@"([A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9]+|[a-z]+)", RegexOptions.Compiled);

    /// <summary>
    /// Stored reference to the member (Field or Property) information on the Prototype.
    /// </summary>
    protected readonly MemberInfo MemberInfo;

    protected readonly TypeCode TypeCode;

    [ObservableProperty]
    private string? _inheritedFields;

    /// <summary>
    /// Whether this property is part of an array.
    /// </summary>
    [ObservableProperty]
    private bool _isArrayElement;

    /// <summary>
    /// Whether this property cannot be changed by the editor.
    /// </summary>
    [ObservableProperty]
    private bool _isFrozen;

    /// <summary>
    /// Name of this property, optional for the case where this property view model is
    /// used in the context of an array.
    /// </summary>
    [ObservableProperty]
    private string _name;

    /// <summary>
    /// Processed version of the name for use in UI viewing, turns "camelCase"
    /// into "Camel Case".
    /// </summary>
    [ObservableProperty]
    private string _uiName;

    /// <summary>
    /// Optional Tooltip for showing exact type information to the user when hovering
    /// the name of the field.
    /// </summary>
    public ObservableCollection<string> Tooltip { get; } = [];

    /// <summary>
    /// Whether this field is a nullable
    /// </summary>
    [ObservableProperty]
    private bool _isNullable = false;

    #region INotifyDataErrorInfo impl

    /// <summary>
    /// List of any errors
    /// </summary>
    private readonly List<string> _errors = [];

    protected void AddError(string error, string propertyName)
    {
        _errors.Add(error);
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    protected void ClearErrors(string propertyName)
    {
        _errors.Clear();
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        return propertyName != null && HasErrors ? _errors : [];
    }

    public bool HasErrors => _errors.Count != 0;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    #endregion

    protected PropertyViewModel(MemberInfo info)
    {
        Name = GetName(info);
        UiName = GetUIName(Name);
        Tooltip.Add(Name);

        TypeCode = GetTypeCode(info);
        MemberInfo = info;
    }

    private static string GetUIName(string name)
    {
        var words = WordRegex.Matches(name)
            .Select(m => m.Value)
            .ToArray();

        var intermediate = string.Join(" ", words);

        return string.Concat(
            char.ToUpper(intermediate[0]).ToString(),
            intermediate.Remove(0, 1));
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

    public abstract void SaveToInstance(IPrototype instance);

    partial void OnIsNullableChanged(bool value)
    {
        const string nullTooltip = "This is a nullable type"; // TODO: Localize?
        if (value)
            Tooltip.Add(nullTooltip);
        else
            Tooltip.Remove(nullTooltip);
    }

    protected void Save(IPrototype instance, object? value)
    {
        if (MemberInfo.MemberType == MemberTypes.Field)
            ((FieldInfo)MemberInfo).SetValue(instance, value);
        else if (MemberInfo.MemberType == MemberTypes.Property)
            ((PropertyInfo)MemberInfo).SetValue(instance, value);
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

    private static TypeCode GetTypeCode(MemberInfo info)
    {
        return info.MemberType switch
        {
            MemberTypes.Field => Type.GetTypeCode(((FieldInfo)info).FieldType),
            MemberTypes.Property => Type.GetTypeCode(((PropertyInfo)info).PropertyType),
            _ => throw new NotImplementedException(), // TODO: Better exception
        };
    }

    private static string GetName(MemberInfo info)
    {
        if (info.TryGetCustomAttribute<DataFieldAttribute>(out var field) &&
            field.Tag != null)
        {
            // Tags override names of the properties
            return field.Tag;
        }

        return info.Name;
    }
}
