
using Robust.Shared.IoC;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Reflection;
using Robust.Shared.Serialization.Manager;

using Robust.Server;
using System.Collections.Generic;
using System.Reflection;
using System;
using Content.Server.Entry;

namespace Content.ProtoEditor.Services;

/// <summary>
/// Initializes and provides access to systems for usage in the editor.
/// Such as, PrototypeManager, LocalisationManager, etc
/// </summary>
public sealed partial class DependencyProvider
{
    /// <summary>
    /// The collection of all known/registered dependencies and the graph between them.
    /// N.b. We can only access this here because the ProtoEditor is set to allow access for
    ///      Robust's internals.
    /// </summary>
    private readonly DependencyCollection _dependencyCollection = default!;

    public DependencyProvider()
    {
        _dependencyCollection = new DependencyCollection();
        InitializeCollection();
        InitializeSystems();
    }

    /// <summary>
    /// Initializes the dependency collection by registering types and their interfaces.
    /// </summary>
    private void InitializeCollection()
    {
        RegisterIoC(_dependencyCollection);
        InitializeForThread();
        _dependencyCollection.BuildGraph();
    }

    /// <summary>
    /// Registers the dependency graph for the current thread.
    /// </summary>
    public void InitializeForThread()
    {
        IoCManager.InitThread(_dependencyCollection);
    }

    /// <summary>
    /// Performs setup and initialization for all systems required for the proto editor to function.
    /// TODO: Consider splitting this out into another DI injected class that relies on the background
    ///       worker in order to make it occur in parallel with the UI loading.
    /// </summary>
    private void InitializeSystems()
    {
        // ####
        // This mimics the RobustIntegrationTest.cs::BaseServer::Init()
        // ####
        var cfg = _dependencyCollection.Resolve<IConfigurationManagerInternal>();
        cfg.Initialize(true);

        // Sets up the configMgr
        // If a config file path was passed, use it literally.
        // This ensures it's working-directory relative
        // (for people passing config file through the terminal or something).
        // Otherwise use the one next to the executable.
        var path = PathHelpers.ExecutableRelativeFile("server_config.toml");
        cfg.LoadFromFile(path); // TODO: We shouldn't need this.

        cfg.LoadCVarsFromAssembly(typeof(BaseServer).Assembly); // Robust.Server
        cfg.LoadCVarsFromAssembly(typeof(IConfigurationManager).Assembly); // Robust.Shared

        CVarDefaultOverrides.OverrideServer(cfg);

        cfg.OverrideConVars(EnvironmentVariables.GetEnvironmentCVars());

        // ####
        // Now we're into BaseServer.cs::BaseServer::Start()
        // ####

        var loc = _dependencyCollection.Resolve<ILocalizationManagerInternal>();
        loc.Initialize();
        //_loc.AddLoadedToStringSerializer(_stringSerializer); // TODO: What is this used for, is it important?

        // ####
        // Now we're into EntryPoint.cs::EntryPoint::Start()
        // ####
        var reflection = _dependencyCollection.Resolve<IReflectionManager>();
        reflection.Initialize();
        /*
            TODO: Ideally we would load both the Client and the Server to ensure we can make any
            prototypes from both. Some odd serialization issues with multiple registrations of the same
            thing, so we can't right now.
        */
        reflection.LoadAssemblies(new List<Assembly>(2)
        {
            AppDomain.CurrentDomain.GetAssemblyByName("Robust.Shared"),
            AppDomain.CurrentDomain.GetAssemblyByName("Robust.Server"),
            AppDomain.CurrentDomain.GetAssemblyByName("Content.Shared"),
            AppDomain.CurrentDomain.GetAssemblyByName("Content.Server"),
        });

        // All the components
        var factory = IoCManager.Resolve<IComponentFactory>();
        factory.DoAutoRegistrations();
        factory.IgnoreMissingComponents("Visuals");
        factory.RegisterIgnore(IgnoredComponents.List);

        var res = _dependencyCollection.Resolve<IResourceManagerInternal>();
        res.Initialize(null);
        res.MountContentDirectory($@"../../RobustToolbox/Resources/");
        res.MountContentDirectory($@"../../Resources/");

        var serialization = _dependencyCollection.Resolve<ISerializationManager>();
        serialization.Initialize();
    }

    /// <summary>
    /// Resolves the dependency from the known collection.
    /// </summary>
    /// <typeparam name="T">Interface type to resolve for.</typeparam>
    /// <returns>Resolved implementration for the requested interface.</returns>
    public T Resolve<T>()
    {
        return _dependencyCollection.Resolve<T>();
    }
}
