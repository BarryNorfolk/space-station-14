using Content.ProtoEditor.ViewModels;

namespace Content.ProtoEditor.Messages;

/// <summary>
/// Message raised when a prototype is selected from the list/hierarchal view so that
/// it can be viewed in the component view.
/// </summary>
/// <param name="prototype">The prototype, if any, that is selected.</param>
public sealed class PrototypeSelectedMessage(PrototypeViewModel? prototype)
{
    /// <summary>
    /// The selected, if any, prototype.
    /// </summary>
    public PrototypeViewModel? Prototype = prototype;
}
