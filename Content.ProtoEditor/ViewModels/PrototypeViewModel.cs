using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.ViewModels;

/// <summary>
/// View model wrapper around the IPrototype and prototype "Kind" (effectively the models).
/// </summary>
/// <param name="prototype">The prototype model from a prototype manager.</param>
/// <param name="kind">The type that this prototype is for.</param>
public sealed partial class PrototypeViewModel(IPrototype prototype, Type kind) : ViewModelBase
{
    /// <summary>
    /// The unique ID of the prototype.
    /// </summary>
    [ObservableProperty]
    private string _id = prototype.ID;

    /// <summary>
    /// The file that contains this particular prototype, used for saving/updating
    /// the model.
    ///
    /// TODO: Populate this somehow.
    /// </summary>
    public string FileLocation = "";

    /// <summary>
    /// The "Kind" of prototype this is for,
    /// e.g. EntityPrototype, AccessPrototype, etc.
    /// </summary>
    public Type Kind = kind;

    /// <summary>
    /// The base instance of the prototype that would be instantiated.
    /// </summary>
    public readonly IPrototype Instance = prototype;
}
