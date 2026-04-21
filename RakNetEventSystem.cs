using System;
using System.Runtime.InteropServices;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using SampSharp.OpenMp.Core;
using SampSharp.OpenMp.Core.Api;
using SampSharp.RakNet.Entities.Interop;

namespace SampSharp.RakNet.Entities;

/// <summary>
/// Мост RakNet-событий в ECS. Регистрирует <c>UnmanagedCallersOnly</c>-методы
/// как callback'и в SampSharp.RakNet.dll, которые синхронно дёргаются на тике
/// open.mp из pawnraknet.dll (через <c>IPawnRakNetEventHandler</c>).
///
/// <para>
/// Event-имена:
/// <list type="bullet">
///   <item><c>OnIncomingPacket(Player, int packetId, BitStream)</c></item>
///   <item><c>OnIncomingRPC(Player, int rpcId, BitStream)</c></item>
///   <item><c>OnIncomingCustomRPC(Player, int rpcId, BitStream)</c></item>
///   <item><c>OnOutgoingPacket(Player, int packetId, BitStream)</c></item>
///   <item><c>OnOutgoingRPC(Player, int rpcId, BitStream)</c></item>
/// </list>
/// Все события возвращают <see cref="bool"/>: <c>false</c> → veto (пакет не уходит / не принимается).
/// </para>
/// </summary>
internal sealed class RakNetEventSystem : ISystem
{
    private static IEventDispatcher? _dispatcher;
    private static IOmpEntityProvider? _entityProvider;
    private static bool _registered;
    private static readonly object _sync = new();

    public RakNetEventSystem(IEventDispatcher dispatcher, IOmpEntityProvider entityProvider,
        SampSharpEnvironment environment)
    {
        lock (_sync)
        {
            _dispatcher = dispatcher;
            _entityProvider = entityProvider;
            RakNetEnvironmentAccessor.Bind(environment);
            if (_registered) return;
            RegisterCallbacks();
            _registered = true;
        }
    }

    private static unsafe void RegisterCallbacks()
    {
        RakNetInterop.RakNet_SetCallback_IncomingPacket(&OnIncomingPacket);
        RakNetInterop.RakNet_SetCallback_IncomingRPC(&OnIncomingRpc);
        RakNetInterop.RakNet_SetCallback_IncomingCustomRPC(&OnIncomingCustomRpc);
        RakNetInterop.RakNet_SetCallback_OutgoingPacket(&OnOutgoingPacket);
        RakNetInterop.RakNet_SetCallback_OutgoingRPC(&OnOutgoingRpc);
    }

    private static EntityId PlayerEntity(int playerId)
    {
        if (playerId < 0 || _entityProvider is null) return default;
        try
        {
            var pool = RakNetEnvironmentAccessor.TryGetPlayerPool();
            if (pool is null) return default;
            var player = pool.Value.Get(playerId);
            if (!player.HasValue) return default;
            return _entityProvider.GetEntity(player);
        }
        catch { return default; }
    }

    // Event dispatch returns object? from Invoke; we treat it as bool where
    // false means "veto". Default (null/true) propagates.
    //
    // BitStream handle is passed as a raw int so legacy [Event] handlers with
    // `int bs` signatures continue to work. Handlers that want a managed wrapper
    // can do `BitStream.Borrow(bsHandle)` at entry.
    private static byte InvokeVeto(string name, int playerId, int id, int bsHandle)
    {
        if (_dispatcher is null) return 1;
        try
        {
            var result = _dispatcher.Invoke(name, PlayerEntity(playerId), id, bsHandle);
            return result is false ? (byte)0 : (byte)1;
        }
        catch
        {
            // Never let an exception veto at the networking layer.
            return 1;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static byte OnIncomingPacket(int playerId, int packetId, int bsHandle)
        => InvokeVeto("OnIncomingPacket", playerId, packetId, bsHandle);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static byte OnIncomingRpc(int playerId, int rpcId, int bsHandle)
        => InvokeVeto("OnIncomingRPC", playerId, rpcId, bsHandle);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static byte OnIncomingCustomRpc(int playerId, int rpcId, int bsHandle)
        => InvokeVeto("OnIncomingCustomRPC", playerId, rpcId, bsHandle);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static byte OnOutgoingPacket(int playerId, int packetId, int bsHandle)
        => InvokeVeto("OnOutgoingPacket", playerId, packetId, bsHandle);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static byte OnOutgoingRpc(int playerId, int rpcId, int bsHandle)
        => InvokeVeto("OnOutgoingRPC", playerId, rpcId, bsHandle);
}

/// <summary>Static holder для IPlayerPool — UnmanagedCallersOnly-методы не могут иметь this.</summary>
internal static class RakNetEnvironmentAccessor
{
    private static IPlayerPool? _pool;
    public static void Bind(SampSharpEnvironment env) => _pool = env?.Core.GetPlayers();
    public static IPlayerPool? TryGetPlayerPool() => _pool;
}
