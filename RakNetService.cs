using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using SampSharp.RakNet.Entities.Interop;

namespace SampSharp.RakNet.Entities;

/// <summary>Реализация <see cref="IRakNetService"/>.</summary>
public sealed class RakNetService : IRakNetService
{
    public bool IsAvailable => RakNetInterop.RakNet_IsAvailable();

    public bool SendPacket(BitStream bs, int playerId,
        PacketPriority priority = PacketPriority.HighPriority,
        PacketReliability reliability = PacketReliability.ReliableOrdered,
        byte orderingChannel = 0)
        => RakNetInterop.RakNet_SendPacket(bs.Id, playerId, (int)priority, (int)reliability, orderingChannel);

    public bool SendPacket(BitStream bs, EntityId player,
        PacketPriority priority = PacketPriority.HighPriority,
        PacketReliability reliability = PacketReliability.ReliableOrdered,
        byte orderingChannel = 0)
        => SendPacket(bs, EntityToPlayerId(player), priority, reliability, orderingChannel);

    public bool SendRpc(BitStream bs, int playerId, int rpcId,
        PacketPriority priority = PacketPriority.HighPriority,
        PacketReliability reliability = PacketReliability.ReliableOrdered,
        byte orderingChannel = 0)
        => RakNetInterop.RakNet_SendRPC(bs.Id, playerId, rpcId, (int)priority, (int)reliability, orderingChannel);

    public bool SendRpc(BitStream bs, EntityId player, int rpcId,
        PacketPriority priority = PacketPriority.HighPriority,
        PacketReliability reliability = PacketReliability.ReliableOrdered,
        byte orderingChannel = 0)
        => SendRpc(bs, EntityToPlayerId(player), rpcId, priority, reliability, orderingChannel);

    public bool EmulateIncomingPacket(BitStream bs, int playerId)
        => RakNetInterop.RakNet_EmulateIncomingPacket(bs.Id, playerId);

    public bool EmulateIncomingPacket(BitStream bs, EntityId player)
        => EmulateIncomingPacket(bs, EntityToPlayerId(player));

    public bool EmulateIncomingRpc(BitStream bs, int playerId, int rpcId)
        => RakNetInterop.RakNet_EmulateIncomingRPC(bs.Id, playerId, rpcId);

    public bool EmulateIncomingRpc(BitStream bs, EntityId player, int rpcId)
        => EmulateIncomingRpc(bs, EntityToPlayerId(player), rpcId);

    public void SetCustomRpc(int rpcId) => RakNetInterop.RakNet_SetCustomRPC(rpcId);
    public bool IsCustomRpc(int rpcId) => RakNetInterop.RakNet_IsCustomRPC(rpcId);

    private static int EntityToPlayerId(EntityId id) => id.IsEmpty ? -1 : id.Handle;
}

/// <summary>
/// Статический facade для extension-методов и мест без DI
/// (см. <see cref="SampSharp.Cef.Entities.CefGlobal"/> для аналогичного паттерна).
/// </summary>
public static class RakNetGlobal
{
    public static bool IsAvailable => RakNetInterop.RakNet_IsAvailable();
}
