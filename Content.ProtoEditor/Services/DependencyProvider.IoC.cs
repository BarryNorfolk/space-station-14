using Content.ProtoEditor.Services.Fakes;
using Robust.Server.Configuration;
using Robust.Server.GameObjects;
using Robust.Server.Localization;
using Robust.Server.Reflection;
using Robust.Server.Serialization;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Exceptions;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Log;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Reflection;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;

namespace Content.ProtoEditor.Services;

/// <summary>
/// IoC side of the dependency provider, registers all implementations we might need.
/// </summary>
public sealed partial class DependencyProvider
{
    /// <summary>
    /// Registers all IoC's with the dependency collection.
    /// </summary>
    /// <param name="deps">Collection to register with.</param>
    private static void RegisterIoC(DependencyCollection deps)
    {
        deps.Register<ISerializationManager, SerializationManager>();
        deps.Register<IDynamicTypeFactory, DynamicTypeFactory>();
        deps.Register<IDynamicTypeFactoryInternal, DynamicTypeFactory>();
        deps.Register<IEntitySystemManager, EntitySystemManager>();
        deps.Register<ILogManager, LogManager>();
        deps.Register<IModLoader, ModLoader>();
        deps.Register<IModLoaderInternal, ModLoader>();
        deps.Register<INetManager, FakeNetManager>(); // Simple NetManager that does nothing
        deps.Register<IRuntimeLog, RuntimeLog>();
        deps.Register<ITaskManager, TaskManager>();
        deps.Register<TaskManager, TaskManager>();
        deps.Register<ITimerManager, TimerManager>();
        deps.Register<IRobustRandom, RobustRandom>();
        deps.Register<IRobustMappedStringSerializer, RobustMappedStringSerializer>();

        deps.Register<IGameTiming, GameTiming>();
        deps.Register<IReflectionManager, ServerReflectionManager>();
        deps.Register<IComponentFactory, ServerComponentFactory>();
        deps.Register<IEntityManager, ServerEntityManager>();
        deps.Register<IResourceManager, ResourceManager>();
        deps.Register<IResourceManagerInternal, ResourceManager>();
        deps.Register<IRobustSerializer, ServerRobustSerializer>();
        deps.Register<IRobustSerializerInternal, ServerRobustSerializer>();
        deps.Register<IConfigurationManager, ServerNetConfigurationManager>();
        deps.Register<IConfigurationManagerInternal, ServerNetConfigurationManager>();
        deps.Register<ILocalizationManager, ServerLocalizationManager>();
        deps.Register<ILocalizationManagerInternal, ServerLocalizationManager>();

        deps.Register<IPrototypeManager, EditorPrototypeManager>();
        deps.Register<IPrototypeManagerInternal, EditorPrototypeManager>();
    }
}
