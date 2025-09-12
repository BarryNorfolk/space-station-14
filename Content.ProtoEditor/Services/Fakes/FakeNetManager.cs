using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Robust.Shared.Network;
using Robust.Shared.ViewVariables;

namespace Content.ProtoEditor.Services.Fakes;

/// <summary>
/// A simple fake net manager for use in the Prototype Editor that does baiscally nothing.
/// </summary>
public sealed class FakeNetManager : INetManager
{
    private sealed class FakeNetChannel(FakeNetManager manager) : INetChannel
    {
        /// <inheritdoc />
        public INetManager NetPeer => manager;

        /// <inheritdoc />
        public long ConnectionId { get; } = 0;

        /// <inheritdoc />
        public IPEndPoint RemoteEndPoint { get; }

        /// <inheritdoc />
        [ViewVariables]
        public NetUserId UserId { get; }

        /// <inheritdoc />
        [ViewVariables]
        public string UserName { get; } = "FakeUserName";

        /// <inheritdoc />
        [ViewVariables]
        public LoginType AuthType { get; }

        /// <inheritdoc />
        public TimeSpan RemoteTimeOffset { get; }

        /// <inheritdoc />
        public TimeSpan RemoteTime { get; }

        /// <inheritdoc />
        [ViewVariables]
        public short Ping { get; } = 0;

        /// <inheritdoc />
        [ViewVariables]
        public bool IsConnected { get; } = true;

        /// <inheritdoc />
        public NetUserData UserData { get; }

        /// <inheritdoc />
        public bool IsHandshakeComplete { get; }

        /// <inheritdoc />
        [ViewVariables]
        public int CurrentMtu { get; } = 1500;

        /// <inheritdoc />
        public T CreateNetMessage<T>()
            where T : NetMessage, new()
        {
            return new T();
        }

        /// <inheritdoc />
        public void SendMessage(NetMessage message) { }

        /// <inheritdoc />
        public void Disconnect(string reason) { }

        /// <inheritdoc />
        public void Disconnect(string reason, bool sendBye) { }
    }

    /// <inheritdoc />
    public bool IsServer { get; private set; } = true;

    /// <inheritdoc />
    public bool IsClient => !IsServer;

    /// <inheritdoc />
    public bool IsRunning { get; } = true;

    /// <inheritdoc />
    public bool IsConnected { get; } = true; // Always connected

    /// <inheritdoc />
    public NetworkStats Statistics => new(0L, 0L, 0L, 0L); // No bytes are ever sent

    private readonly List<FakeNetChannel> _channels = [];

    /// <inheritdoc />
    public IEnumerable<INetChannel> Channels => _channels;

    /// <inheritdoc />
    public int ChannelCount => _channels.Count;

    /// <inheritdoc />
    public int Port { get; }

    private readonly Dictionary<Type, long> _bandwidthUsage = [];
    public IReadOnlyDictionary<Type, long> MessageBandwidthUsage => _bandwidthUsage;

    /// <inheritdoc />
    public void ResetBandwidthMetrics() { }

    /// <inheritdoc />
    public void Initialize(bool isServer) { }

    /// <inheritdoc />
    public void StartServer() { }

    /// <inheritdoc />
    public void Shutdown(string reason) { }

    /// <inheritdoc />
    public void ProcessPackets() { }

    /// <inheritdoc />
    public void ServerSendToAll(NetMessage message) { }

    /// <inheritdoc />
    public void ServerSendMessage(NetMessage message, INetChannel recipient) { }

    /// <inheritdoc />
    public void ServerSendToMany(NetMessage message, List<INetChannel> recipients) { }

    /// <inheritdoc />
    public void ClientSendMessage(NetMessage message) { }

    private readonly List<Func<NetConnectingArgs, Task>> _connectingEvent = [];
    /// <inheritdoc />
    public event Func<NetConnectingArgs, Task> Connecting
    {
        add => _connectingEvent.Add(value);
        remove => _connectingEvent.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<NetChannelArgs>? Connected;

    /// <inheritdoc />
    public event EventHandler<NetDisconnectedArgs>? Disconnect;

    /// <inheritdoc />
    public void RegisterNetMessage<T>(ProcessMessage<T>? rxCallback = null,
        NetMessageAccept accept = NetMessageAccept.Both)
        where T : NetMessage, new()
    { }

    /// <inheritdoc />
    public T CreateNetMessage<T>() where T : NetMessage, new()
    {
        return new T();
    }
}

