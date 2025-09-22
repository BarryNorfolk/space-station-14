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

/// <summary>
/// Message raised when an EntProtoId control requests us to show the linked Prototype.
/// </summary>
/// <param name="protoId">The prototype id that is selected.</param>
public sealed class PrototypeIdSelectedMessage(string protoId)
{
    /// <summary>
    /// The ID of the prototype that has been selected
    /// </summary>
    public string ProtoId = protoId;
}
