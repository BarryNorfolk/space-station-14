using System;
using System.Collections.Generic;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.Services;

public sealed class EditorPrototypeManager : PrototypeManager
{
    public EditorPrototypeManager()
    {
        RegisterIgnore("shader");
        RegisterIgnore("uiTheme");
        RegisterIgnore("font");
    }

    public override void LoadDefaultPrototypes(Dictionary<Type, HashSet<string>>? changed = null)
    {
        LoadDirectory(new("/EnginePrototypes/"), changed: changed);
        LoadDirectory(new("/Prototypes/"), changed: changed); // TODO: Make possibly configurable?
        ResolveResults();
    }
}
