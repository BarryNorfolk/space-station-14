using System.Diagnostics.Metrics;
using Robust.Server;
using Robust.Server.Configuration;
using Robust.Server.Console;
using Robust.Server.DataMetrics;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Server.Localization;
using Robust.Server.Placement;
using Robust.Server.Player;
using Robust.Server.Prototypes;
using Robust.Server.Reflection;
using Robust.Server.Replays;
using Robust.Server.Scripting;
using Robust.Server.Serialization;
using Robust.Server.ServerHub;
using Robust.Server.ServerStatus;
using Robust.Server.Upload;
using Robust.Server.ViewVariables;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Reflection;
using Robust.Shared.Replays;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Upload;
using Robust.Shared.ViewVariables;

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
        /*
            While it would be very beneficial to fake/mock a lot of the larger classes out,
            and I've tried, there's a lot of logic and inter dependencies which mean a lot of fakes
            that have to be manually created.

            If we get to the point of wanting to cleaning up this dependency list so we aren't directly
            reliant on literally everything from the server, we can revisit fakes.
        */

        // Custom PrototypeManagers for this Editor
        deps.Register<IPrototypeManager, EditorPrototypeManager>();
        deps.Register<IPrototypeManagerInternal, EditorPrototypeManager>();

        SharedIoC.RegisterIoC(deps);

        deps.Register<IBaseServer, BaseServer>();
        deps.Register<IBaseServerInternal, BaseServer>();
        deps.Register<BaseServer, BaseServer>();
        deps.Register<IGameTiming, GameTiming>();
        deps.Register<IReflectionManager, ServerReflectionManager>();
        deps.Register<IConsoleHost, ServerConsoleHost>();
        deps.Register<IServerConsoleHost, ServerConsoleHost>();
        deps.Register<IComponentFactory, ServerComponentFactory>();
        deps.Register<IConGroupController, ConGroupController>();
        deps.Register<IMapManager, NetworkedMapManager>();
        deps.Register<IMapManagerInternal, NetworkedMapManager>();
        deps.Register<INetworkedMapManager, NetworkedMapManager>();
        deps.Register<IEntityManager, ServerEntityManager>();
        deps.Register<IEntityNetworkManager, ServerEntityManager>();
        deps.Register<IServerEntityNetworkManager, ServerEntityManager>();
        deps.Register<IPlacementManager, PlacementManager>();
        deps.Register<IPlayerManager, PlayerManager>();
        deps.Register<ISharedPlayerManager, PlayerManager>();
        //deps.Register<IPrototypeManager, ServerPrototypeManager>(); // Replaced by EditorPrototypeManager
        //deps.Register<IPrototypeManagerInternal, ServerPrototypeManager>(); // Replaced by EditorPrototypeManager
        deps.Register<IResourceManager, ResourceManager>();
        deps.Register<IResourceManagerInternal, ResourceManager>();
        deps.Register<EntityManager, ServerEntityManager>();
        deps.Register<IServerEntityManager, ServerEntityManager>();
        deps.Register<IServerGameStateManager, ServerGameStateManager>();
        deps.Register<IReplayRecordingManager, ReplayRecordingManager>();
        deps.Register<IReplayRecordingManagerInternal, ReplayRecordingManager>();
        deps.Register<IServerReplayRecordingManager, ReplayRecordingManager>();
        deps.Register<IServerNetManager, NetManager>();
        deps.Register<IStatusHost, StatusHost>();
        deps.Register<ISystemConsoleManager, SystemConsoleManager>();
        deps.Register<ITileDefinitionManager, TileDefinitionManager>();
        deps.Register<IViewVariablesManager, ServerViewVariablesManager>();
        deps.Register<IServerViewVariablesInternal, ServerViewVariablesManager>();
        deps.Register<IWatchdogApi, WatchdogApi>();
        deps.Register<IWatchdogApiInternal, WatchdogApi>();
        deps.Register<IScriptHost, ScriptHost>();
        deps.Register<IMetricsManager, MetricsManager>();
        deps.Register<IMetricsManagerInternal, MetricsManager>();
        deps.Register<IMeterFactory, MetricsManager>();
        deps.Register<IAuthManager, AuthManager>();
        deps.Register<HubManager, HubManager>();
        deps.Register<IRobustSerializer, ServerRobustSerializer>();
        deps.Register<IRobustSerializerInternal, ServerRobustSerializer>();
        deps.Register<IConfigurationManager, ServerNetConfigurationManager>();
        deps.Register<INetConfigurationManager, ServerNetConfigurationManager>();
        deps.Register<IConfigurationManagerInternal, ServerNetConfigurationManager>();
        deps.Register<IServerNetConfigurationManager, ServerNetConfigurationManager>();
        deps.Register<INetConfigurationManagerInternal, ServerNetConfigurationManager>();
        deps.Register<IGamePrototypeLoadManager, GamePrototypeLoadManager>();
        deps.Register<GamePrototypeLoadManager>();
        deps.Register<NetworkResourceManager>();
        deps.Register<IHttpClientHolder, HttpClientHolder>();
        deps.Register<UploadedContentManager>();
        deps.Register<IHWId, DummyHWId>();
        deps.Register<ILocalizationManager, ServerLocalizationManager>();
        deps.Register<ILocalizationManagerInternal, ServerLocalizationManager>();
    }
}
