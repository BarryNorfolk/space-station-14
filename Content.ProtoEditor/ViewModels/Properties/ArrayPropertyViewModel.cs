
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Content.ProtoEditor.ViewModels.Properties;

// TODO: Reorder property
/// <summary>
/// Special view model for handling arrays of properties for view models.
/// </summary>
/// <param name="name">Name of this array property.</param>
/// <param name="elementType">The type of the elements within this array.</param>
/// <param name="value">Initial array values for this property.</param>
/// <param name="onAddItemFunc">Function to use when an item is requested to be added,
///                             supplied by the main factory.</param>
public sealed partial class ArrayPropertyViewModel(string? name, Type elementType, List<PropertyViewModel>? value, Func<Type, PropertyViewModel> onAddItemFunc) : PropertyViewModel(name)
{
    /// <summary>
    /// The elements of the array.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PropertyViewModel> _elements = value != null ? new(value) : [];

    /// <summary>
    /// The type of the elements contained within the array.
    /// </summary>
    private readonly Type _elementType = elementType;

    /// <summary>
    /// Stored function for when an item is requested to be added to the array.
    /// </summary>
    private readonly Func<Type, PropertyViewModel> _onAddItemFunc = onAddItemFunc;

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
        var property = _onAddItemFunc(_elementType);
        property.IsArrayElement = true;
        Elements.Add(property);
    }
}

