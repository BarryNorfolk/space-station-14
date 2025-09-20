using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.ViewModels.Properties;

// TODO: Reorder property
/// <summary>
/// Special view model for handling arrays of properties for view models.
/// </summary>
/// <param name="info">Member information for the prototype.</param>
/// <param name="elementType">The type of the elements within this array.</param>
/// <param name="value">Initial array values for this property.</param>
/// <param name="onAddItemFunc">
/// Function to use when an item is requested to be added,
/// supplied by the main factory.
/// </param>
public sealed partial class ArrayPropertyViewModel(
    MemberInfo info,
    Type elementType,
    List<PropertyViewModel>? value,
    Func<MemberInfo, Type, PropertyViewModel> onAddItemFunc) : PropertyViewModel(info)
{
    /// <summary>
    /// The type of the elements contained within the array.
    /// </summary>
    private readonly Type _elementType = elementType;

    /// <summary>
    /// Stored function for when an item is requested to be added to the array.
    /// </summary>
    private readonly Func<MemberInfo, Type, PropertyViewModel> _onAddItemFunc = onAddItemFunc;

    /// <summary>
    /// The elements of the array.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PropertyViewModel> _elements =
        value != null ? new ObservableCollection<PropertyViewModel>(value) : [];

    /// <summary>
    /// Command bound to the RemoveElementButton button, which removes the specified element from
    /// the array.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    [RelayCommand]
    private void RemoveItem(PropertyViewModel? item)
    {
        if (item != null && Elements.Contains(item))
            Elements.Remove(item);
    }

    /// <summary>
    /// Command bound to the AddItem button in the ArrayPropertyViewModel, which adds a new
    /// (null) property to the array.
    /// </summary>
    [RelayCommand]
    private void AddItem()
    {
        // TODO: The MemberInfo here is wrong as that's the Array's information.
        //       Fix it.
        var property = _onAddItemFunc(MemberInfo, _elementType);
        property.IsArrayElement = true;
        Elements.Add(property);
    }

    // TODO: Fix this to work
    public override void SaveToInstance(IPrototype instance) { Save(instance, Elements); }
}
