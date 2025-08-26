using System.Threading.Tasks;
using Content.IntegrationTests;
using NUnit.Framework;
using Robust.UnitTesting;

namespace Content.ProtoEditor.Services;

/// <summary>
/// Provides access, and initializes, the server assemblies required for resolving components,
/// services and such.
///
/// TODO: Relies on the logic from integration tests to get the server, this might be pretty slow
/// in terms of performance and we might be able to do better.
/// </summary>
public sealed class AssemblyProvider
{
    /// <summary>
    /// The resolved server instance.
    /// </summary>
    private RobustIntegrationTest.ServerIntegrationInstance? _server;

    /// <summary>
    /// The log handler for the server.
    /// </summary>
    private PoolTestLogHandler? _serverLogHandler;

    public AssemblyProvider()
    {
        Initialization = InitializeAsync();
    }

    /// <summary>
    /// Exposes the initialization task so that other async tasks may await on it.
    /// </summary>
    public Task Initialization { get; private set; }

    /// <summary>
    /// Initializes this class.
    /// Awaits the pool manager to load the relevant server assembly and log handler.
    /// </summary>
    /// <returns>The initialization task</returns>
    public async Task InitializeAsync()
    {
        var (server, serverLogHandler) = await PoolManager.GenerateServer(new PoolSettings(), TestContext.Out);
        _server = server;
        _serverLogHandler = serverLogHandler;
    }

    /// <summary>
    /// Exposes the server instance.
    /// </summary>
    /// <returns>The resolved server instance.</returns>
    public RobustIntegrationTest.ServerIntegrationInstance? Server()
    {
        return _server;
    }
}
