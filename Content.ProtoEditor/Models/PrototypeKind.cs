using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Content.ProtoEditor.Models;

/// <summary>
/// Simple wrapper around a prototype Kind to enable nice visibility in UI,
/// and filtering of the list of prototypes.
/// </summary>
/// <param name="kind">The actual underlying type for the IPrototype kind.</param>
public sealed class PrototypeKind(Type kind)
{
    /// <summary>
    /// The actual type for the IPrototype, as determined by the IPrototypeManager.
    /// </summary>
    public readonly Type Kind = kind;

    /// <summary>
    /// Full name of the prototype kind.
    /// E.g. "Content.Shared.Store.ListingPrototype".
    /// </summary>
    public string Name => Kind.FullName ?? Kind.ToString();

    /// <summary>
    /// Simple short name for viewing in UI.
    /// E.g. "Content.Shared.Store.ListingPrototype" becomes "ListingPrototype".
    /// </summary>
    public string ShortName => Kind.ShortDisplayName();
}
